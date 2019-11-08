using ObjectsLibrary;
using RemotingServicesLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace Server {
    public class Server {

        // Default values
        private String _id = "LISBOA";
        private String _url = "tcp://localhost:8086/LISBOA";
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

        private Boolean _freeze = false;
        
        public Server() {
            _locations = new Dictionary<String, Location>();
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();

            setServer(_id);
            serversConfig();
        }

        public Server(int port, String id, String url, int max_faults, int min_delay, int max_delay, String obj) {
            _id = id;
            _url = url;
            _port = port;
            _max_faults = max_faults;
            _min_delay = min_delay;
            _max_delay = max_delay;

            _locations = new Dictionary<String, Location>();
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();

            setServer(obj);
            serversConfig();
        }

        public String ID {
            get { return _id; }
        }

        public String URL { 
            get { return _url; }
        }

        public IDictionary<String, String> Clients {
            get { return _clients; }
        }

        public IDictionary<String, int> Operations {
            get { return _vectorClock; }
            set { _vectorClock = value; }
        }


        private void setServer(String obj) {
            _channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(_channel, false);

            ServerService serverService = new ServerService(this);
            RemotingServices.Marshal(serverService, obj, typeof(ServerService));
            
            Console.WriteLine("[SERVER:" + _id + "] " + _url + " Delay: " + _min_delay + "ms to " + _max_delay + "ms");
        }

        public void serversConfig() {
            _otherServers = new Dictionary<String, IServerService>();
            _vectorClock = new Dictionary<String, int>();
            _otherServersLastVectorClock = new Dictionary<String, IDictionary<String, int>>();
            _otherServersLastReceivedMeetings = new Dictionary<String, List<Meeting>>();

              Console.WriteLine("|========== Servers ==========|");
            Console.WriteLine(_url + "  [THIS SERVER]");
            foreach (String serverUrl in ConfigurationManager.AppSettings) {
                if (!serverUrl.Equals(_url)) {
                    Console.WriteLine(serverUrl);
                    IServerService serverServ = (IServerService)Activator.GetObject(typeof(IServerService), serverUrl);
                    _otherServers.Add(serverUrl, serverServ);
                }
                _vectorClock.Add(serverUrl, 0);
            }

            foreach (String serverUrl in ConfigurationManager.AppSettings) {
                if (!serverUrl.Equals(_url)) {
                    _otherServersLastVectorClock.Add(serverUrl, _vectorClock);
                    _otherServersLastReceivedMeetings.Add(serverUrl, new List<Meeting>());
                }
            }
        }
        public void clientConnect(String username, String clientUrl) {
            _clients.Add(username, clientUrl);
            informNewClient(username, clientUrl);
        }

        public void informNewClient(String username, String clientUrl) {
            Console.WriteLine("[NEW CLIENT: " + username + " ]");
            foreach (IServerService serverServ in _otherServers.Values)
                serverServ.receiveNewClient(username, clientUrl);
        }

        public void receiveNewClient(String username, String clientUrl) {
            _clients.Add(username, clientUrl);
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots) {
            Meeting meeting = new Meeting(username, topic, minAtt, slots);
            _meetings.Add(meeting.Topic, meeting);
            incrementVectorClock();
            replicateChanges(meeting);
            Console.WriteLine("[CLIENT:" + username + "] Created meeting " + topic);
            return meeting;
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            Meeting meeting = new Meeting(username, topic, minAtt, slots, invitees);
            _meetings.Add(meeting.Topic, meeting);
            incrementVectorClock();
            replicateChanges(meeting);
            Console.WriteLine("[CLIENT:" + username + "] Created meeting " + topic);
            return meeting;
        }

        public Meeting getMeeting(String topic) {
            return _meetings[topic];
        }

        public bool checkMeetingStatusChange(Meeting meeting) {
            return meeting.checkStatusChange(_meetings[meeting.Topic]);
        }

        // TODO Check if meeting exists and do returns to client properly
        public Meeting joinMeetingSlot(String topic, String slot, String username) {
            if (_meetings[topic].joinSlot(slot, username)) {
                incrementVectorClock();
                replicateChanges(_meetings[topic]);
                Console.WriteLine("[CLIENT:" + username + "] Joined meeting " + topic + " on slot " + slot);
                return _meetings[topic];
            }
            return null;
        }

        public void closeMeeting(String topic) {
            Console.WriteLine("Close Meeting" + topic);
            Meeting meeting = _meetings[topic];

            if (meeting.checkClose()) {
                while (meeting.MStatus != Meeting.Status.BOOKED) {
                    Slot slot = meeting.mostCapacitySlot();
                    if (slot == null)
                        break;
                    foreach (Room room in _locations[slot.Location].roomWithCapacity(slot.NJoined)) {
                        if (room.checkRoomFree(slot.Date)) {
                            if (room.bookMeeting(slot.Date, meeting)) {
                                Console.WriteLine("Room " + room.Name);
                                slot.PickedRoom = room;
                                meeting.PickedSlot = slot;
                                meeting.MStatus = Meeting.Status.BOOKED;
                                meeting.cleanInvalidSlots();
                                incrementVectorClock();
                                replicateChanges(meeting);
                                return;
                            }
                        }
                    }
                    if (meeting.MStatus == Meeting.Status.OPEN) {
                        meeting.invalidSlot(slot);
                    }
                }
                //theres no available room so we need to exclude some clients
                if (meeting.MStatus == Meeting.Status.OPEN) {
                    meeting.cleanInvalidSlots();
                    while (meeting.MStatus != Meeting.Status.BOOKED) {
                        Slot slot = meeting.mostCapacitySlot();
                        if (slot == null)
                            break;
                        foreach (Room room in _locations[slot.Location].Rooms) {
                            if (room.checkRoomFree(slot.Date)) {
                                if (room.bookMeeting(slot.Date, meeting)) {
                                    Console.WriteLine("Room " + room.Name);
                                    slot.PickedRoom = room;
                                    meeting.PickedSlot = slot;
                                    meeting.MStatus = Meeting.Status.BOOKED;
                                    meeting.cleanInvalidSlots();

                                    if (room.Capacity < meeting.PickedSlot.NJoined)
                                        meeting.PickedSlot.Joined = excludeClients(room.Capacity, meeting.PickedSlot);

                                    incrementVectorClock();
                                    replicateChanges(meeting);
                                    return;
                                }
                            }
                        }

                    }
                }
                if (meeting.MStatus == Meeting.Status.OPEN) {
                    meeting.MStatus = Meeting.Status.CANCELLED;
                    incrementVectorClock();
                    replicateChanges(meeting);
                }
            }
        }

        public List<String> excludeClients(int roomCapacity, Slot pickedSlot) {
            List<String> joinedClients = pickedSlot.Joined;
            List<String> finalClients = new List<String>();
            int counter = 0;
            foreach (String client in joinedClients) {
                if (counter <= roomCapacity) {
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

        public void replicateChanges(Meeting meeting) {
            IDictionary<String, List<Meeting>> meetingsToSend = new Dictionary<String, List<Meeting>>();        // <Server URL, Server Meetings to send>
            List<Meeting> thisMeetings = new List<Meeting>();
            thisMeetings.Add(meeting);
            meetingsToSend.Add(_url, thisMeetings);
            foreach (KeyValuePair<String, IServerService> server in _otherServers) {                            // Loops through other servers
                foreach (KeyValuePair<String, int> vectorClock in _otherServersLastVectorClock[server.Key]) {   // Loops through server last vector clock
                    if (!vectorClock.Key.Equals(_url) && !vectorClock.Key.Equals(server.Key)) {                 // If URL not mine and not receiver's
                        if (_vectorClock[vectorClock.Key] > vectorClock.Value) {
                            meetingsToSend.Add(vectorClock.Key, _otherServersLastReceivedMeetings[vectorClock.Key]);
                        }
                    }
                }
                server.Value.receiveChanges(_url, _vectorClock, meetingsToSend);
            }
        }

        public void receiveChanges(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings) {
            IDictionary<String, List<Meeting>> meetingsToResend = new Dictionary<String, List<Meeting>>();      // <Server URL, Server Meetings to send>
            foreach (KeyValuePair<String, int> serverClock in vectorClock) {                                    // Loops through clocks
                if (serverClock.Value > _vectorClock[serverClock.Key]) {                                        // If received server clock value > stored server clock value
                    foreach (Meeting meeting in meetings[serverClock.Key]) {                                    // Set meetings from that server
                        if (_meetings.ContainsKey(meeting.Topic))
                            _meetings[meeting.Topic] = meeting;
                        else
                            _meetings.Add(meeting.Topic, meeting);
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
            _otherServersLastVectorClock[serverUrl] = vectorClock;                                              // Update last received vector clock
        }

        public void addRoom(String roomLocation, int capacity, String name) {
            _locations[roomLocation].addRoom(new Room(name, capacity));
        }

        public void addLocation(String location_name, Location location) {
            _locations.Add(location_name, location);
        }

        public String status() {
            String s = "[SERVER: " + _id + "] has the following meetings and locations: \n";

            foreach(Meeting meeting in _meetings.Values) {
                s += meeting.ToString();
            }

            foreach(Location location in _locations.Values){
                s += location.ToString();
            }

            return s;
        }

        public void freeze() {
            _freeze = true;
        }

        public Boolean Freeze {
            get { return _freeze; }
        }

        public void checkDelay() {
            Random random = new Random();
            int delay = random.Next(_min_delay, _max_delay);
            Thread.Sleep(delay);
        }

        static void Main(string[] args) {
            if(args.Length == 0) {
                Server server = new Server();
            }
            else{
                Server server = new Server(Int32.Parse(args[0]), args[1], args[2], Int32.Parse(args[3]), Int32.Parse(args[4]), Int32.Parse(args[5]), args[6]);
            }
            Console.ReadLine();
        }
    }
}
