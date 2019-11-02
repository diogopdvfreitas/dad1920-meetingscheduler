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
        private String _id = "SERVER";
        private String _url = "tcp://localhost:8086/SERVER";
        private int _port = 8086;
        private int _max_faults = 0;
        private int _min_delay = 0;
        private int _max_delay = 0;

        TcpChannel _channel;

        private IDictionary<String, Location> _locations;               // <Location Name, Location>
        private IDictionary<String, Meeting> _meetings;                 // <Meeting Topic, Meeting>
        private IDictionary<String, String> _clients;                   // <Client Username, Client URL>
        private IDictionary<String, IServerService> _otherServers;      // <Server URL, Server Service>
        private IDictionary<String, ServerSnapshot> _replicas;          // <Server URL, Server Snapshot>
        private List<String> _delayedMessages;                          // msgs delayed while frozen 
        private List<String> _sentMessageServers;

        private Timer _replicationTimer;
        private Boolean _freeze = false;
        
        public Server() {
            _locations = new Dictionary<String, Location>();
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();
            _otherServers = new Dictionary<String, IServerService>();
            _replicas = new Dictionary<String, ServerSnapshot>();
            _delayedMessages = new List<String>();

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
            _clients = new Dictionary<String, String>();
            _otherServers = new Dictionary<String, IServerService>();
            _replicas = new Dictionary<String, ServerSnapshot>();
            _delayedMessages = new List<String>();

            setServer();
            serversConfig();
        }

        public String ID {
            get { return _id; }
        }

        public String URL { 
            get { return _url; }
        }

        private void setServer() {
            _channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(_channel, false);

            ServerService serverService = new ServerService(this);
            RemotingServices.Marshal(serverService, _id, typeof(ServerService));
            
            Console.WriteLine("Server " + _id + " created at port " + _port + " with delay from " + _min_delay + " to " + _max_delay);
        }

        public void serversConfig() {
            foreach (String serverUrl in ConfigurationManager.AppSettings) {
                Console.WriteLine("Servers:" + serverUrl);
                Console.WriteLine(_url);
                if (serverUrl != _url) {
                    //String[] urlAttributes = serverUrl.Split(new Char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    IServerService serverServ = (IServerService)Activator.GetObject(typeof(IServerService), serverUrl);
                    _otherServers.Add(serverUrl, serverServ); //neste momento a key é o url, mas acho que nao vai ser, provavelmente vai ser o id
                }
            }
        }

        public void clientConnect(String username, String clientUrl) {
            _clients.Add(username, clientUrl);
        }

        public IDictionary<String, String> Clients {
            get { return _clients; }
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots) {
            Meeting meeting = new Meeting(username, topic, minAtt, slots);
            Console.WriteLine(meeting.ToString());
            _meetings.Add(meeting.Topic, meeting);
            return meeting;
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            Meeting meeting = new Meeting(username, topic, minAtt, slots, invitees);
            Console.WriteLine(meeting.ToString());
            _meetings.Add(meeting.Topic, meeting);
            return meeting;
        }

        public Meeting getMeeting(String topic) {
            return _meetings[topic];
        }

        public bool checkMeetingStatusChange(Meeting meeting) {
            return meeting.checkStatusChange(_meetings[meeting.Topic]);
        }

        public bool joinMeetingSlot(String topic, Slot slot, String username) {
            if (_meetings[topic].joinSlot(slot, username))
                return true;
            return false;
        }

        public void closeMeeting(String topic) {
            _meetings[topic].close();
        }

        public void addRoom(String roomLocation, int capacity, String name) {
            _locations[roomLocation].addRoom(new Room(name, capacity));
        }

        public void initReplication() {
            _replicationTimer = new Timer(new TimerCallback(sendReplica), null, 5000, 500);
        }

        public void sendReplica(object state) {
            ServerSnapshot replica = new ServerSnapshot(_id, _meetings);
            foreach (IServerService serverService in _otherServers.Values) {
                receiveReplica(replica);
            }
        }

        public void receiveReplica(ServerSnapshot replica) {
            if (_meetings.Count != replica.Meetings.Count) {
                _meetings = replica.Meetings;
                Console.WriteLine("Current Server State doesn't correspond to Server Snapshot from server " + replica.OriginServerId + ".\nUpdating server state.");
            }
        }

        public void freeze() {
            _freeze = true;
        }

        public Boolean Freeze {
            get { return _freeze; }
        }

        public List<String> DelayedMessages {
            get { return _delayedMessages; }
        }

        public void addDelayedMessage() {
            //_delayedMessages.Add();
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
