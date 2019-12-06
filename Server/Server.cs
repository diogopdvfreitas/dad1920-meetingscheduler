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
using ExceptionsLibrary;

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
        private IDictionary<String, IServerService> _allServers;                                  // <Server URL, IServerService>
        private IDictionary<String, int> _vectorClock;                                              // <Server URL, Clock Counter>
        private IDictionary<String, IDictionary<String, int>> _otherServersLastVectorClock;         // <Server URL, Known Server Vector Clock>
        private IDictionary<String, List<Meeting>> _otherServersLastReceivedMeetings;               // <Server URL, Last Received Meeting from Server>
        private IList<String> _unreachServers;
        private Boolean _freeze = false;

        public delegate void ReplicationDelegate(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings);
        private IList<ReplicationDelegate> delegates = new List<ReplicationDelegate>();

        private KeyValuePair<String, IServerService> _leader;
        private int _leaderPort;
        private bool _IAmLeader = false;

        private int _lastCloseTicket = 0;
        private IDictionary<String, Meeting> _pendingCloses;
        private KeyValuePair<int, String> _lastGrantedTicketByTheLeader;

        //Leader:
        private KeyValuePair<int, String> _lastGrantedCloseTicket;

        public Server() {
            _locations = new Dictionary<String, Location>();
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();
            _pendingCloses = new Dictionary<String, Meeting>();


            setServer(_objName);
            serversConfig();

            _leader = new KeyValuePair<string, IServerService>(_url, (IServerService)Activator.GetObject(typeof(IServerService), _url));
            _leaderPort = _port;
            selectLeader();
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
            _pendingCloses = new Dictionary<String, Meeting>();

            setServer(obj);
            serversConfig();

            _leader = new KeyValuePair<string, IServerService>(_url, (IServerService)Activator.GetObject(typeof(IServerService), _url));
            _leaderPort = _port;
            selectLeader();
        }

        public IDictionary<String, String> Clients {
            get { return _clients; }
        }

        public IDictionary<String, IServerService> Servers {
            get { return _allServers; }
        }

        public IDictionary<String, int> VectorClock {
            get { return _vectorClock; }
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
            _allServers = new Dictionary<String, IServerService>();
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
                    _allServers.Add(serverUrl, serverServ);
                }
                else {
                    IServerService serverServ = (IServerService)Activator.GetObject(typeof(IServerService), serverUrl);
                    _allServers.Add(serverUrl, serverServ);
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

        public IDictionary<String, int> clientConnect(String username, String clientUrl) {
            lock (_clients)
                _clients.Add(username, clientUrl);
            Console.WriteLine("[CONNECT] CLIENT:" + username + " connected to this server");
            foreach (IServerService serverServ in _otherServers.Values)
                serverServ.receiveNewClient(username, clientUrl);
            return _vectorClock;
        }

        public void receiveNewClient(String username, String clientUrl) {
            lock (_clients)
                _clients.Add(username, clientUrl);
        }

        public MeetingMessage createMeeting(String username, String topic, int minAtt, List<Slot> slots) {
            if (!_meetings.ContainsKey(topic)) {
                Meeting meeting = new Meeting(username, topic, minAtt, slots);
                lock (_meetings)
                    _meetings.Add(meeting.Topic, meeting);
                incrementVectorClock();
                replicateChanges(meeting);
                Console.WriteLine("[CLIENT:" + username + "] Created meeting " + topic);
                return new MeetingMessage(_url, _vectorClock, meeting);
            }
            throw new AlreadyExistingMeetingException("Couldn't create meeting.\nMeeting with topic " + topic + " already exists!");
        }

        public MeetingMessage createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            if (!_meetings.ContainsKey(topic)) {
                Meeting meeting = new Meeting(username, topic, minAtt, slots, invitees);
                lock (_meetings)
                    _meetings.Add(meeting.Topic, meeting);
                incrementVectorClock();
                replicateChanges(meeting);
                Console.WriteLine("[CLIENT:" + username + "] Created meeting " + topic);
                return new MeetingMessage(_url, _vectorClock, meeting);
            }
            throw new AlreadyExistingMeetingException("Couldn't create meeting.\nMeeting with topic " + topic + " already exists!");
        }

        public MeetingMessage joinMeetingSlot(String topic, String slot, String username) {
            if (_meetings.ContainsKey(topic)) {
                lock (_meetings[topic]) {
                    if (_meetings[topic].MStatus == Meeting.Status.OPEN) {
                        if (_meetings[topic].joinSlot(slot, username)) {
                            incrementVectorClock();
                            replicateChanges(_meetings[topic]);
                            Console.WriteLine("[CLIENT:" + username + "] Joined meeting " + topic + " on slot " + slot);
                            return new MeetingMessage(_url, _vectorClock, _meetings[topic]);
                        }
                        else
                            throw new SlotNotFoundException("Couldn't join meeting. \nDesired slot not found.");
                    }
                    else
                        throw new ClosedMeetingException("Couldn't join meeting. \nMeeting " + topic + " already closed.");
                }
            }
            throw new UnknownMeetingException("Couldn´t join meeting. \nMeeting " + topic + " unknow.");
        }

        public MeetingMessage closeMeeting(String topic, String username) {
            Meeting meeting;
            try {
                meeting = _meetings[topic];
            } catch (KeyNotFoundException) { 
                 throw new UnknownMeetingException("Meeting " + topic + " unknow."); 
            }

            if (meeting.MStatus != Meeting.Status.OPEN)
                throw new ClosedMeetingException("Meeting " + topic + " already closed.");

            lock (meeting) {
                if (checkIfCanClose(meeting, username)) {
                    if (meeting.checkClose()) {
                        while (meeting.MStatus != Meeting.Status.BOOKED) {
                            Slot slot = meeting.mostCapacitySlot();
                            if (slot == null || slot.NJoined == 0) {
                                break;
                            }
                            foreach (Room room in _locations[slot.Location].roomWithCapacity(slot.NJoined)) {
                                if (room.checkRoomFree(slot.Date)) {
                                    if (room.bookMeeting(slot.Date, meeting)) {
                                        Slot pickedSlot = new Slot(slot.Location, slot.Date);
                                        pickedSlot.PickedRoom = room;
                                        pickedSlot.Joined = slot.Joined;
                                        meeting.PickedSlot = pickedSlot;
                                        meeting.MStatus = Meeting.Status.BOOKED;
                                        meeting.cleanInvalidSlots();
                                        incrementVectorClock();
                                        replicateChanges(meeting);
                                        Console.WriteLine("[CLIENT:" + username + "] Closed meeting " + meeting.Topic +
                                            ". Selected Slot is " + meeting.PickedSlot + " in Room " + meeting.PickedSlot.PickedRoom);
                                        processPendingCloses();
                                        return new MeetingMessage(_url, _vectorClock, _meetings[topic]);
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
                                            Slot pickedSlot = new Slot(slot.Location, slot.Date);
                                            pickedSlot.PickedRoom = room;
                                            pickedSlot.Joined = slot.Joined;
                                            meeting.PickedSlot = pickedSlot;
                                            meeting.MStatus = Meeting.Status.BOOKED;
                                            meeting.cleanInvalidSlots();

                                            if (room.Capacity < meeting.PickedSlot.NJoined)
                                                meeting.PickedSlot.Joined = excludeClients(room.Capacity, meeting.PickedSlot);
                                            
                                            incrementVectorClock();
                                            replicateChanges(meeting);
                                            Console.WriteLine("[CLIENT:" + username + "] Closed meeting " + meeting.Topic +
                                                ". Selected Slot is " + meeting.PickedSlot + " in Room " + meeting.PickedSlot.PickedRoom);
                                            processPendingCloses();
                                            return new MeetingMessage(_url, _vectorClock, _meetings[topic]);
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
                            processPendingCloses();
                            return new MeetingMessage(_url, _vectorClock, _meetings[topic]);
                        }
                    }
                    else {
                        meeting.MStatus = Meeting.Status.CANCELLED;
                        incrementVectorClock();
                        replicateChanges(meeting);
                        Console.WriteLine("[CLIENT:" + username + "] Closed meeting " + meeting.Topic + " but meeting was cancelled");
                        processPendingCloses();

                        throw new NotEnoughAttendeesExceptions("Meeting Canceled " + topic + ".\nMinimum number of Atendees not yet reached!");
                    }
                }
                else
                    _pendingCloses.Add(meeting.Topic, meeting);
            }
            return null;
        }

        public Meeting getMeeting(String topic) {
            if (_meetings.ContainsKey(topic))
                return _meetings[topic];
            throw new UnknownMeetingException("Meeting " + topic + " unknow.");

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

                        } catch (SocketException) {
                            if (!_unreachServers.Contains(server.Key)) {
                                _unreachServers.Add(server.Key);
                                Console.WriteLine("[Unreached server: " + server.Key + "]");

                                if (_leader.Key.Equals(server.Key))
                                    selectLeader();
                            }
                        }
                    });
                    thread.Start();
                }
            }
            int threadCounter = 0;
            while (threadCounter != _max_faults) {       //It waits for max_fauls responses since it is us plus _max_faults (f+1) in order to tolerate _max_faults faults
                int index = WaitHandle.WaitAny(handles);
                handles[index] = new AutoResetEvent(false);
                threadCounter++;
            }
        }

        public void receiveChanges(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings) {
            lock (this) {
                bool changes = false;

                Console.WriteLine("[RECEIVED FROM: " + serverUrl + "]");
                IDictionary<String, List<Meeting>> meetingsToResend = new Dictionary<String, List<Meeting>>();      // <Server URL, Server Meetings to send>
                foreach (KeyValuePair<String, int> serverClock in vectorClock) {                                    // Loops through clocks
                    if (serverClock.Value > _vectorClock[serverClock.Key]) {                                        // If received server clock value > stored server clock value
                        foreach (Meeting meeting in meetings[serverClock.Key]) {                                    // Set meetings from that server
                            //Check if it is a new close operation that can be executed now
                            if (meeting.MStatus != Meeting.Status.OPEN & meeting.CloseTicket == (_lastCloseTicket + 1)) {
                                _lastCloseTicket = meeting.CloseTicket;
                                if (_meetings.ContainsKey(meeting.Topic))
                                    if (_meetings[meeting.Topic].MStatus == Meeting.Status.OPEN)
                                        _meetings[meeting.Topic] = meeting;
                                    else
                                        _meetings.Add(meeting.Topic, meeting);
                                if (meeting.MStatus == Meeting.Status.BOOKED) {
                                    foreach (Room room in _locations[meeting.PickedSlot.Location].Rooms)
                                        if (room.Name.Equals(meeting.PickedSlot.PickedRoom.Name))
                                            room.bookMeeting(meeting.PickedSlot.Date, meeting);
                                }

                                processPendingCloses();
                            }
                            //Check if it is a close operation that needs to wait
                            else if (meeting.MStatus != Meeting.Status.OPEN & meeting.CloseTicket > (_lastCloseTicket + 1)) {
                                _pendingCloses.Add(meeting.Topic, meeting);
                            }
                            //Check if it is a delayed close operation
                            else if (meeting.MStatus != Meeting.Status.OPEN & meeting.CloseTicket < _lastCloseTicket) {
                                Console.WriteLine("[Delayed Close Meeting. Closed Discard]");
                            }
                            //It´s not a close operation
                            else {

                                if (_meetings.ContainsKey(meeting.Topic))
                                    _meetings[meeting.Topic] = meeting;
                                else
                                    _meetings.Add(meeting.Topic, meeting);
                            }
                            changes = true;
                        }
                        _vectorClock[serverClock.Key] = serverClock.Value;                                      // Update local server clock
                        _otherServersLastReceivedMeetings[serverClock.Key] = meetings[serverClock.Key];             // Update local server last received meetings

                    }
                    if (serverClock.Value < _vectorClock[serverClock.Key] & !serverClock.Key.Equals(_url)) {                                        // If received server clock value < stored server clock value
                        meetingsToResend.Add(serverClock.Key, _otherServersLastReceivedMeetings[serverClock.Key]);  // Server who sent is delayed and should receive the last meetings regarding this server clock
                    }

                    }
                    if (meetingsToResend.Count != 0)
                        try {
                            _otherServers[serverUrl].receiveChanges(_url, _vectorClock, meetingsToResend);
                        }
                        catch (SocketException) {
                            if (!_unreachServers.Contains(serverUrl)) {
                                _unreachServers.Add(serverUrl);
                                Console.WriteLine("[Unreached server: " + serverUrl + "]");

                            if (_leader.Key.Equals(serverUrl))
                                selectLeader();
                        }
                    }
                _otherServersLastVectorClock[serverUrl] = vectorClock;                                           // Update last received vector clock

                if (changes) {
                    sendToServers(vectorClock, meetings);
                }
            }
        }

        public void addRoom(String roomLocation, int capacity, String name) {
            _locations[roomLocation].addRoom(new Room(name, capacity));
        }

        public void addLocation(String location_name, Location location) {
            _locations.Add(location_name, location);
        }

        public IDictionary<String, int> updateServer() {
            KeyValuePair<String, IDictionary<String, int>> mostUpdatedServerVectorClock = new KeyValuePair<String, IDictionary<String, int>>(_url, _vectorClock);
            foreach (KeyValuePair<String, IServerService> server in _otherServers) {
                bool newVC = false;
                IDictionary<String, int> serverVectorClock = server.Value.getVectorClock();
                foreach (String serverURL in serverVectorClock.Keys) {
                    if (serverVectorClock[serverURL] > _vectorClock[serverURL])
                        newVC = true;
                    if (serverVectorClock[serverURL] < _vectorClock[serverURL])
                        newVC = false;
                }
                if (newVC)
                    mostUpdatedServerVectorClock = new KeyValuePair<String, IDictionary<String, int>>(server.Key, serverVectorClock);
            }

            if (!mostUpdatedServerVectorClock.Value.Equals(_vectorClock))
                _otherServers[mostUpdatedServerVectorClock.Key].getUpdatedMeetingsFromUpdatedServer(_url, _vectorClock);
            
            _vectorClock = mostUpdatedServerVectorClock.Value;
            return _vectorClock;
        }

        public void getUpdatedMeetingsFromUpdatedServer(String requestingServerURL, IDictionary<String, int> vectorClock) {
            IDictionary<String, List<Meeting>> meetingsToSend = new Dictionary<String, List<Meeting>>();        // <Server URL, Server Meetings to send>
            foreach (String serverURL in vectorClock.Keys) {
                if (!serverURL.Equals(requestingServerURL) && !serverURL.Equals(_url)) {
                    if (_vectorClock[serverURL] > vectorClock[serverURL]) {
                        meetingsToSend.Add(serverURL, _otherServersLastReceivedMeetings[serverURL]);
                    }
                }
            }
            _otherServers[requestingServerURL].receiveChanges(_url, _vectorClock, meetingsToSend);
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


        public void selectLeader() {
            if (_unreachServers.Contains(_leader.Key)) {
                _leader = new KeyValuePair<string, IServerService>(_url, (IServerService)Activator.GetObject(typeof(IServerService), _url));
                _leaderPort = _port;
            }
            foreach (KeyValuePair<String, IServerService> server in _otherServers) {
                if (!_unreachServers.Contains(server.Key)) {
                    String[] serverUrl = server.Key.Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);
                    int serverPort = Int32.Parse(serverUrl[2]);
                    if (_leaderPort < serverPort) {
                        _leader = new KeyValuePair<string, IServerService>(server.Key, server.Value);
                        _leaderPort = serverPort;

                    }
                }
            }

            if (_leaderPort == _port) {
                _IAmLeader = true;
                if(_lastGrantedTicketByTheLeader.Key == 0)
                    _lastGrantedCloseTicket = new KeyValuePair<int, String>(0, _url);
                else
                    _lastGrantedCloseTicket = new KeyValuePair<int, String>(_lastGrantedTicketByTheLeader.Key, _lastGrantedTicketByTheLeader.Value);

            }else {
                _leader.Value.selectNewLeader();
            }
            Console.WriteLine("[LEADER: " + _leader.Key + "]");
        }

        public void selectNewLeader() {
            if (!_IAmLeader) {
                _unreachServers.Add(_leader.Key);
                selectLeader();
            }
        }



        public KeyValuePair<int, String> grantCloseTicket(String serverUrl) {
            if (_IAmLeader) {
                int ticket = _lastGrantedCloseTicket.Key;
                ticket++;
                _lastGrantedCloseTicket = new KeyValuePair<int, String>(ticket, serverUrl);
                Console.WriteLine("[SERVER: " + _lastGrantedCloseTicket.Value + " ASKED FOR CLOSE TICKET. GIVEN: " + _lastGrantedCloseTicket.Key + "]");

                spreadNewTicket(_lastGrantedCloseTicket);

                return _lastGrantedCloseTicket;
            }
            return new KeyValuePair<int, string>((_lastCloseTicket + 1), serverUrl);
        }

        public void spreadNewTicket(KeyValuePair<int, String> newGrantedTicket) {
            foreach (KeyValuePair<String, IServerService> server in _otherServers) {
                if (!_unreachServers.Contains(server.Key) & !server.Key.Equals(_leader)) {
                    try {
                        server.Value.newGrantedTicket(_url, newGrantedTicket);

                    }
                    catch (SocketException) {
                        _unreachServers.Add(server.Key);
                        Console.WriteLine("[Unreached server: " + server.Key + "]");
                    }
                }
            }
        }

        public void newGrantedTicket(string leader, KeyValuePair<int, String> newGrantedTicketByLeader) {
            if (!_leader.Key.Equals(leader))
                _leader = new KeyValuePair<String, IServerService>(leader, _otherServers[leader]);

            if (_lastGrantedTicketByTheLeader.Key < newGrantedTicketByLeader.Key) {
                _lastGrantedTicketByTheLeader = new KeyValuePair<int, String>(newGrantedTicketByLeader.Key, newGrantedTicketByLeader.Value);
                spreadNewTicket(newGrantedTicketByLeader);
            }
        }


        //This is when the server is asked by a client to do a close (the logic to do a close sent by another server is in receiveChanges function)
        public bool checkIfCanClose(Meeting meeting, String username) {
            int ticket;
            try {
                ticket = _leader.Value.grantCloseTicket(_url).Key;
                meeting.CloseTicket = ticket;
                meeting.CloseUser = username;
                if (ticket == (_lastCloseTicket + 1)) {
                    _lastCloseTicket = ticket;
                    Console.WriteLine("[CLOSE TICKET: " + _lastCloseTicket+"]");
                    return true;
                }
            }catch (SocketException) {
                _unreachServers.Add(_leader.Key);
                selectLeader();
                closeMeeting(meeting.Topic, username);
            }
            return false;
        }

        public void processPendingCloses() {
            if (_pendingCloses.Count != 0) {
                foreach (Meeting meeting in _pendingCloses.Values) {
                    if (meeting.CloseTicket == (_lastCloseTicket + 1) & meeting.MStatus == Meeting.Status.OPEN)
                        closeMeeting(meeting.Topic, meeting.CloseUser);
                }
            }
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

    }
}