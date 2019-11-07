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

        private IDictionary<String, Location> _locations;               // <Location Name, Location>
        private IDictionary<String, Meeting> _meetings;                 // <Meeting Topic, Meeting>
        private IDictionary<String, bool> _meetingsLockStatus;           // <Meeting Topic, Meeting Lock>
        private IDictionary<String, String> _clients;                   // <Client Username, Client URL>
        private IDictionary<String, IServerService> _otherServers;      // <Server URL, IServerService>
        private IDictionary<String, int> _vectorTimeStamp;              //<ServerUrl, timeStamp> //??? key tbm podera ser o id

        private IDictionary<String, IDictionary<int, DelayedMessage>> _delayedMessagesFromServers; //<Server Url,<Future TimeStamp, Delayed Message>>
       // private List<String> _sentMessageServers;

       //private Timer _replicationTimer;
        private Boolean _freeze = false;
        
        public Server() {
            _locations = new Dictionary<String, Location>();
            _meetings = new Dictionary<String, Meeting>();
            _meetingsLockStatus = new Dictionary<String, bool>();
            _clients = new Dictionary<String, String>();

            setServer();
            serversConfig();
        }

        public Server(int port, String id, String url, int max_faults, int min_delay, int max_delay) {
            _id = id;
            _url = url;
            _port = port;
            _max_faults = max_faults;
            _min_delay = min_delay;
            _max_delay = max_delay;

            _locations = new Dictionary<String, Location>();
            _meetings = new Dictionary<String, Meeting>();
            _meetingsLockStatus = new Dictionary<String, bool>();
            _clients = new Dictionary<String, String>();

            setServer();
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

        public IDictionary<String, int> VectorTimeStamp {
            get { return _vectorTimeStamp; }
            set { _vectorTimeStamp = value; }
        }


        private void setServer() {
            _channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(_channel, false);

            ServerService serverService = new ServerService(this);
            RemotingServices.Marshal(serverService, _id, typeof(ServerService));
            
            Console.WriteLine("[SERVER:" + _id + "] " + _url + " Delay: " + _min_delay + "ms to " + _max_delay + "ms");
        }

        public void serversConfig() {
            _otherServers = new Dictionary<String, IServerService>();
            _vectorTimeStamp = new Dictionary<String, int>();

            Console.WriteLine("|========== Servers ==========|");
            Console.WriteLine("[THIS SERVER]  " + _url);
            foreach (String serverUrl in ConfigurationManager.AppSettings) {
                if (!serverUrl.Equals(_url)) {
                    Console.WriteLine(serverUrl);
                    IServerService serverServ = (IServerService)Activator.GetObject(typeof(IServerService), serverUrl);
                    _otherServers.Add(serverUrl, serverServ);
                }
                _vectorTimeStamp.Add(serverUrl, 0);
            }
        }
        public void clientConnect(String username, String clientUrl) {
            _clients.Add(username, clientUrl);
            informNewClient(username, clientUrl);
        }

        public void informNewClient(String username, String clientUrl) {
            foreach (IServerService serverServ in _otherServers.Values)
                serverServ.receiveNewClient(username, clientUrl);

        }

        public void receiveNewClient(String username, String clientUrl) {
            _clients.Add(username, clientUrl);
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots) {
            Meeting meeting = new Meeting(username, topic, minAtt, slots);
            _meetings.Add(meeting.Topic, meeting);
            Console.WriteLine("[CLIENT:" + username + "] Created meeting " + topic);
            incrementTimeStamp();
            sendMeetings();
            return meeting;
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            Meeting meeting = new Meeting(username, topic, minAtt, slots, invitees);
            _meetings.Add(meeting.Topic, meeting);
            Console.WriteLine("[CLIENT:" + username + "] Created meeting " + topic);
            incrementTimeStamp();
            sendMeetings();
            return meeting;
        }

        public Meeting getMeeting(String topic) {
            return _meetings[topic];
        }

        public bool checkMeetingStatusChange(Meeting meeting) {
            return meeting.checkStatusChange(_meetings[meeting.Topic]);
        }

        public Meeting joinMeetingSlot(String topic, String slot, String username) { // TODO Como funciona uma chamada remota, é apenas uma chamade e depois executa as outras ou espera pelo resultado da chamada remota. Devemos utilizar Threads para fazer as alterações remotas concorrentemente o mais depressa possível?
            if (_meetings[topic].joinSlot(slot, username)) {
                Console.WriteLine("[CLIENT:" + username + "] Joined meeting " + topic + " on slot " + slot);
                incrementTimeStamp();
                sendMeetings();
                return _meetings[topic];
            }
            return null;
        }

        public void closeMeeting(String topic) {
            _meetings[topic].close();
        }

        public void incrementTimeStamp() {
            _vectorTimeStamp[_url] += 1;
        }

        //sendMeeting: sends the meeting new state to the other servers with url and vectorTimeStamp 
        public void sendMeetings() {
            Console.WriteLine("Meetings sent");
            foreach (IServerService serverServ in _otherServers.Values) {
                serverServ.receiveMeeting(_vectorTimeStamp, _meetings);
            }
        }

        public bool checkVectorTimeStamp(IDictionary<String, int> vectorTimeStamp) {
            foreach (KeyValuePair<String, int> serverTimeStamp in vectorTimeStamp) {
                if (serverTimeStamp.Key != _url & _vectorTimeStamp[serverTimeStamp.Key] > serverTimeStamp.Value) {
                    return false;
                }
            }
            return true;
        }

        //receiveMeeting: receives the meeting new status and the send server vectorTimeStamp
        public void receiveMeeting(IDictionary<String, int> vectorTimeStamp, IDictionary<String, Meeting> meetings) {
            if (checkVectorTimeStamp(vectorTimeStamp)) {
                int timeStamp = _vectorTimeStamp[_url];
                _vectorTimeStamp = vectorTimeStamp;

                _vectorTimeStamp[_url] = timeStamp;
            }
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
                Server server = new Server(Int32.Parse(args[0]), args[1], args[2], Int32.Parse(args[3]), Int32.Parse(args[4]), Int32.Parse(args[5]));
            }
            Console.ReadLine();
        }
    }
}
