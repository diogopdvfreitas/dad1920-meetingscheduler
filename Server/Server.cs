using ObjectsLibrary;
using RemotingServicesLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Net.Sockets;

namespace Server {
    public class Server {

        // Default values
        private String _id = "LISBOA";
        private String _url = "tcp://localhost:8086/server1";
        private String _objName = "server1";
        private int _port = 8086;
        private int _max_faults = 0;
        private int _min_delay = 0;
        private int _max_delay = 0;

        TcpChannel _channel;

        private IDictionary<String, Location> _locations;                                           // <Location Name, Location>
        private IDictionary<String, Meeting> _meetings;                                             // <Meeting Topic, Meeting>
        private IDictionary<String, String> _clients;                                               // <Client Username, Client URL>
        private IDictionary<String, IServerService> _otherServers;                                  // <Server URL, IServerService>
        private IDictionary<String, int> _vectorClock;                                              // <Server URL, Clock Counter>
        private IDictionary<String, IDictionary<String, int>> _otherServersLastVectorClock;         // <Server URL, Known Server Vector Clock>
        private IDictionary<String, List<Meeting>> _otherServersLastReceivedMeetings;               // <Server URL, Last Received Meeting from Server>
        private IList<String> _unreachServers;
        private Boolean _freeze = false;

        public delegate void ReplicationDelegate(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings);
        private IList<ReplicationDelegate> delegates = new List<ReplicationDelegate>();


        public Server() {
            _locations = new Dictionary<String, Location>();
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();

            setServer(_objName);
            serversConfig();
        }

        public Server(int port, String id, String url, int max_faults, int min_delay, int max_delay, String obj) {
            _id = id;
            _url = url;
            _port = port;
            _objName = obj;
            _max_faults = max_faults;
            _min_delay = min_delay;
            _max_delay = max_delay;

            _locations = new Dictionary<String, Location>();
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();

            setServer(obj);
            serversConfig();
        }

        public IDictionary<String, String> Clients {
            get { return _clients; }
        }

        private void setServer(String obj) {
            _channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(_channel, false);

            ServerService serverService = new ServerService(this);
            RemotingServices.Marshal(serverService, obj, typeof(ServerService));

            Console.WriteLine("[SERVER:" + _id + "] " + _url + " Delay: " + _min_delay + "ms to " + _max_delay + "ms\n");
        }

        public void serversConfig() {
            _otherServers = new Dictionary<String, IServerService>();
            _vectorClock = new Dictionary<String, int>();
            _otherServersLastVectorClock = new Dictionary<String, IDictionary<String, int>>();
            _otherServersLastReceivedMeetings = new Dictionary<String, List<Meeting>>();
            _unreachServers = new List<String>();

            Console.WriteLine("|========== Servers List ==========|");
            Console.WriteLine(_url + "  [THIS SERVER]");
            foreach (String serverUrl in ConfigurationManager.AppSettings) {
                if (!serverUrl.Equals(_url)) {
                    Console.WriteLine(serverUrl);
                    IServerService serverServ = (IServerService)Activator.GetObject(typeof(IServerService), serverUrl);
                    _otherServers.Add(serverUrl, serverServ);
                }
                _vectorClock.Add(serverUrl, 0);
            }
            Console.WriteLine();
            foreach (String serverUrl in ConfigurationManager.AppSettings) {
                if (!serverUrl.Equals(_url)) {
                    _otherServersLastVectorClock.Add(serverUrl, _vectorClock);
                    _otherServersLastReceivedMeetings.Add(serverUrl, new List<Meeting>());
                }
            }
        }

        public void clientConnect(String username, String clientUrl) {
            _clients.Add(username, clientUrl);
            Console.WriteLine("[CONNECT] CLIENT:" + username + " connected to this server");
            foreach (IServerService serverServ in _otherServers.Values)
                serverServ.receiveNewClient(username, clientUrl);
        }

        public void receiveNewClient(String username, String clientUrl) {
            lock (_clients) {
                _clients.Add(username, clientUrl);
            }
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots) {
            if (!_meetings.ContainsKey(topic)) {
                Meeting meeting = new Meeting(username, topic, minAtt, slots);
                _meetings.Add(meeting.Topic, meeting);
                incrementVectorClock();
                replicateChanges(meeting);
                Console.WriteLine("[CLIENT:" + username + "] Created meeting " + topic);
                return meeting;
            }
            return null;
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            if (!_meetings.ContainsKey(topic)) {
                Meeting meeting = new Meeting(username, topic, minAtt, slots, invitees);
                _meetings.Add(meeting.Topic, meeting);
                incrementVectorClock();
                replicateChanges(meeting);
                Console.WriteLine("[CLIENT:" + username + "] Created meeting " + topic);
                return meeting;
            }
            return null;
        }

        public Meeting joinMeetingSlot(String topic, String slot, String username) {
            if (_meetings[topic].joinSlot(slot, username)) {
                incrementVectorClock();
                replicateChanges(_meetings[topic]);
                Console.WriteLine("[CLIENT:" + username + "] Joined meeting " + topic + " on slot " + slot);
                return _meetings[topic];
            }
            return null;
        }

        public Meeting closeMeeting(String topic, String username) {
            Meeting meeting = _meetings[topic];

            if (meeting.checkClose()) {
                while (meeting.MStatus != Meeting.Status.BOOKED) {
                    Slot slot = meeting.mostCapacitySlot();
                    if (slot == null || slot.NJoined == 0) {
                        break;
                    }
                    foreach (Room room in _locations[slot.Location].roomWithCapacity(slot.NJoined)) {
                        if (room.checkRoomFree(slot.Date)) {
                            if (room.bookMeeting(slot.Date, meeting)) {
                                slot.PickedRoom = room;
                                meeting.PickedSlot = slot;
                                meeting.MStatus = Meeting.Status.BOOKED;
                                meeting.cleanInvalidSlots();
                                incrementVectorClock();
                                replicateChanges(meeting);
                                Console.WriteLine("[CLIENT:" + username + "] Closed meeting " + meeting.Topic +
                                    ". Selected Slot is " + meeting.PickedSlot + " in Room " + meeting.PickedSlot.PickedRoom);
                                return meeting;
                            }
                        }
                    }
                    if (meeting.MStatus == Meeting.Status.OPEN) {
                        meeting.invalidSlot(slot);
                    }
                }
                // Theres no available room so we need to exclude some clients
                if (meeting.MStatus == Meeting.Status.OPEN) {
                    meeting.cleanInvalidSlots();
                    while (meeting.MStatus != Meeting.Status.BOOKED) {
                        Slot slot = meeting.mostCapacitySlot();

                        if (slot == null || slot.NJoined == 0) {
                            break;
                        }
                        foreach (Room room in _locations[slot.Location].Rooms) {
                            if (room.checkRoomFree(slot.Date)) {
                                if (room.bookMeeting(slot.Date, meeting)) {
                                    slot.PickedRoom = room;
                                    meeting.PickedSlot = slot;
                                    meeting.MStatus = Meeting.Status.BOOKED;
                                    meeting.cleanInvalidSlots();

                                    if (room.Capacity < meeting.PickedSlot.NJoined)
                                        meeting.PickedSlot.Joined = excludeClients(room.Capacity, meeting.PickedSlot);

                                    incrementVectorClock();
                                    replicateChanges(meeting);
                                    Console.WriteLine("[CLIENT:" + username + "] Closed meeting " + meeting.Topic +
                                        ". Selected Slot is " + meeting.PickedSlot + " in Room " + meeting.PickedSlot.PickedRoom);
                                    return meeting;
                                }
                            }
                        }
                        if (meeting.MStatus == Meeting.Status.OPEN) {
                            meeting.invalidSlot(slot);
                        }
                    }
                }
                if (meeting.MStatus == Meeting.Status.OPEN) {
                    meeting.MStatus = Meeting.Status.CANCELLED;
                    incrementVectorClock();
                    replicateChanges(meeting);
                    Console.WriteLine("[CLIENT:" + username + "] Closed meeting " + meeting.Topic + " but meeting was cancelled");
                    return meeting;
                }
            } else {
                meeting.MStatus = Meeting.Status.CANCELLED;
                incrementVectorClock();
                replicateChanges(meeting);
                Console.WriteLine("[CLIENT:" + username + "] Closed meeting " + meeting.Topic + " but meeting was cancelled");

                return meeting;

            }
            return null;
        }

        public Meeting getMeeting(String topic) {
            if (_meetings.ContainsKey(topic))
                return _meetings[topic];
            return null;
        }

        public bool checkMeetingStatusChange(Meeting meeting) {
            return meeting.checkStatusChange(_meetings[meeting.Topic]);
        }

        public List<String> excludeClients(int roomCapacity, Slot pickedSlot) {
            List<String> joinedClients = pickedSlot.Joined;
            List<String> finalClients = new List<String>();
            int counter = 0;
            foreach (String client in joinedClients) {
                if (counter < roomCapacity) {
                    finalClients.Add(client);
                    counter += 1;
                }
                else
                    break;
            }
            return finalClients;
        }

        public void incrementVectorClock() {
            _vectorClock[_url]++;
        }

        //when we call replicate changes we already did the changes
        public void replicateChanges(Meeting meeting) {
            IDictionary<String, List<Meeting>> meetingsToSend = new Dictionary<String, List<Meeting>>();        // <Server URL, Server Meetings to send>
            List<Meeting> thisMeetings = new List<Meeting>();
            thisMeetings.Add(meeting);
            meetingsToSend.Add(_url, thisMeetings);

            foreach (KeyValuePair<String, IServerService> server in _otherServers) {                            // Loops through other servers
                foreach (KeyValuePair<String, int> vectorClock in _otherServersLastVectorClock[server.Key]) {   // Loops through server last vector clock
                    if (!vectorClock.Key.Equals(_url) && !vectorClock.Key.Equals(server.Key)) {                 // If URL not mine and not receiver's
                        if (_vectorClock[vectorClock.Key] > vectorClock.Value) {                                //Check if I have received a message that the receiver did not receive from the other servers
                            meetingsToSend.Add(vectorClock.Key, _otherServersLastReceivedMeetings[vectorClock.Key]);
                        }
                    }
                }
            }
            sendToServers(_vectorClock, meetingsToSend);
        }

        public void sendToServers(IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings) {
            AutoResetEvent[] handles = new AutoResetEvent[_otherServers.Count];
            for (int i = 0; i < _otherServers.Count; i++) {
                handles[i] = new AutoResetEvent(false);
            }

            int t = 0;
            foreach (KeyValuePair<String, IServerService> server in _otherServers) {
                if (!_unreachServers.Contains(server.Key)) {
                    Thread thread = new Thread(() => {
                        try {

                            server.Value.receiveChanges(_url, vectorClock, meetings);
                            handles[t].Set();
                            t++;

                        }
                        catch (SocketException) {
                            _unreachServers.Add(server.Key);
                            Console.WriteLine("[Unreached server: " + server.Key + "]");
                        }
                    });
                    thread.Start();
                }
            }
            int threadCounter = 0;
            while (threadCounter != _max_faults) {       //It waits for max_fauls responses since it is us plus _max_faults (f+1) in order to tolerate _max_faults faults
                int index = WaitHandle.WaitAny(handles);
                threadCounter++;
            }
        }

        public void receiveChanges(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings) {
            bool changes = false;
            Console.WriteLine("RECEIVED FROM: " + serverUrl);
            IDictionary<String, List<Meeting>> meetingsToResend = new Dictionary<String, List<Meeting>>();      // <Server URL, Server Meetings to send>
            foreach (KeyValuePair<String, int> serverClock in vectorClock) {                                    // Loops through clocks
                if (serverClock.Value > _vectorClock[serverClock.Key]) {                                        // If received server clock value > stored server clock value
                    foreach (Meeting meeting in meetings[serverClock.Key]) {                                    // Set meetings from that server
                        if (_meetings.ContainsKey(meeting.Topic))
                            _meetings[meeting.Topic] = meeting;
                        else
                            _meetings.Add(meeting.Topic, meeting);
                        changes = true;
                    }
                    _vectorClock[serverClock.Key] = serverClock.Value;                                          // Update local server clock
                    _otherServersLastReceivedMeetings[serverClock.Key] = meetings[serverClock.Key];             // Update local server last received meetings
                }
                if (serverClock.Value < _vectorClock[serverClock.Key]) {                                        // If received server clock value < stored server clock value
                    meetingsToResend.Add(serverClock.Key, _otherServersLastReceivedMeetings[serverClock.Key]);  // Server who sent is delayed and should receive the last meetings regarding this server clock
                }

            }
            if (meetingsToResend.Count != 0)
                _otherServers[serverUrl].receiveChanges(_url, _vectorClock, meetingsToResend);
            _otherServersLastVectorClock[serverUrl] = vectorClock;                                           // Update last received vector clock

            if (changes) {
                sendToServers(vectorClock, meetings);
            }
        }

        public void addRoom(String roomLocation, int capacity, String name) {
            _locations[roomLocation].addRoom(new Room(name, capacity));
        }

        public void addLocation(String location_name, Location location) {
            _locations.Add(location_name, location);
        }

        public String status() {
            String s = _id + " stored Meetings:\n";
            foreach(Meeting meeting in _meetings.Values) {
                s += meeting.status();
            }

            s += _id + " stored Locations:\n";
            foreach(Location location in _locations.Values){
                s += location.ToString();
            }

            return s;
        }

        public void freeze() {
            Console.WriteLine("Freezing Server " + _id + ".\n");
            _freeze = true;
        }

        public Boolean Freeze {
            get { return _freeze; }
        }

        public void checkFreeze() {
            if (Freeze) {
                Console.WriteLine("Server " + _id + " still frozen.\n");
                lock (this) {
                    while(Freeze) {
                        Monitor.Wait(this);
                    }
                }
            }
        }

        public void unfreeze() {
            Console.WriteLine("Server " + _id + " unfreezing. \n");
            lock (this) {
                Monitor.PulseAll(this);
            }
            _freeze = false;
        }

        public void checkDelay() {
            Random random = new Random();
            int delay = random.Next(_min_delay, _max_delay);
            Thread.Sleep(delay);
            checkFreeze();
        }

        static void Main(string[] args) {
            if (args.Length == 0) {
                Server server = new Server();
            }
            else {
                Server server = new Server(Int32.Parse(args[0]), args[1], args[2], Int32.Parse(args[3]), Int32.Parse(args[4]), Int32.Parse(args[5]), args[6]);
            }
            Console.ReadLine();
        }

        //the first leader is the server with the highest object name
        public void selectFirstLeader() {
            String highestObjName = _objName;
            /*foreach(String serverUrl in _otherServers.Keys)
            String[] server_url = commandAttr[2].Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);
            String server_IP = server_url[1];*/


        }


    }
}
