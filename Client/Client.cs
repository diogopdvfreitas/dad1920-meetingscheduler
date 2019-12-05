using System;
using System.Collections.Generic;
using ObjectsLibrary;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using RemotingServicesLibrary;
using System.Runtime.Remoting;
using ClientLibrary;
using ExceptionsLibrary;


namespace Client {
    public class Client : ClientAPI {

        //default values
        private String _username = "Rita";
        private String _url = "tcp://localhost:8080/CLIENT";
        private int _port = 8080;
        private String _serverUrl = "tcp://localhost:8080/server1";

        private TcpChannel _channel;

        private ClientService _clientService;
        private IServerService _serverService;

        private IDictionary<String, Meeting> _clientMeetings;           // <Meeting Topic, Meeting>
        private IDictionary<String, IClientService> _otherClients;      // <Client 
        private IDictionary<String, int> _vectorClock;                  // <Server URL, Clock Counter>

        private List<String> _auxToInvites; //auxiliary list of the clients we have to send invites to
        private List<String> _auxMeetingAlredySent; //auxiliary list of the clients that have already send the invite to us

        public Client() {
            _clientMeetings = new Dictionary<String, Meeting>();
            _otherClients = new Dictionary<String, IClientService>();
            _auxToInvites = new List<String>();
            _auxMeetingAlredySent = new List<String>();

            setClient("CLIENT");
            _vectorClock = connectToServer();
        }

        public Client(String username, String clientUrl, String serverUrl) {
            _url = clientUrl;
            String[] clientUrlSplit = clientUrl.Split(new Char[] {':', '/'}, StringSplitOptions.RemoveEmptyEntries);
            _port = Int32.Parse(clientUrlSplit[2]);
            _serverUrl = serverUrl;
            _username = username;
            

            _clientMeetings = new Dictionary<String, Meeting>();
            _otherClients = new Dictionary<String, IClientService>();
            _auxToInvites = new List<String>();
            _auxMeetingAlredySent = new List<String>();

            setClient(clientUrlSplit[3]);
            _vectorClock = connectToServer();
        }

        public void setClient(String objID) {
            _channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(_channel, false);

            _clientService = new ClientService(this);
            RemotingServices.Marshal(_clientService, objID, typeof(ClientService));

            Console.WriteLine("[CLIENT:" + _username + "] " + _url + "\n");
        }

        public IDictionary<String, int> connectToServer() {
            _serverService = (IServerService) Activator.GetObject(typeof(IServerService), _serverUrl);
            IDictionary<String, int> serverVectorClock = _serverService.clientConnect(_username, _url);
            getRegisteredClients();

            Console.WriteLine("[CONNECT] Connected to server at "+ _serverUrl);
            return serverVectorClock;
        }

        public void getRegisteredClients() {
            IDictionary<String, String> registeredClients = _serverService.getRegisteredClients();
            foreach (KeyValuePair<String, String> client in registeredClients) {
                if (!_otherClients.ContainsKey(client.Key)) {
                    IClientService clientServ = (IClientService)Activator.GetObject(typeof(IClientService), client.Value);
                    _otherClients.Add(client.Key, clientServ);
                }
            }
        }

        public bool checkServerNeedsUpdate() {
            IDictionary<String, int> serverVectorClock = _serverService.getVectorClock();
            bool serverNeedsUpdate = false;
            foreach (String serverURL in _vectorClock.Keys) {
                if (_vectorClock[serverURL] > serverVectorClock[serverURL])
                    serverNeedsUpdate = true;
            }
            return serverNeedsUpdate;
        }

        public void listMeetings() {
            while (checkServerNeedsUpdate()) { _serverService.updateServer(); };
            Console.WriteLine("|========== Meetings ==========|");

            String list = "";
            List<String> meetingStatusChanged = new List<String>();

            lock (meetingStatusChanged) {
                foreach (Meeting meeting in _clientMeetings.Values) {
                    if (_serverService.checkMeetingStatusChange(meeting)) {
                        meetingStatusChanged.Add(meeting.Topic);
                    }
                }
            }

            lock (_clientMeetings) {
                foreach (String topic in meetingStatusChanged) {
                    _clientMeetings[topic] = _serverService.getMeeting(topic);
                }
            }

            foreach (Meeting meeting in _clientMeetings.Values) {
                list += meeting.ToString(); 
            }
            Console.WriteLine(list);  
        }

        public void createMeeting(String topic, int minAtt, List<Slot> slots) {
            while (checkServerNeedsUpdate()) { _serverService.updateServer(); };
            Meeting meeting = _serverService.createMeeting(_username, topic, minAtt, slots).Meeting;
            if (meeting != null) {
                lock (_clientMeetings) {
                    _clientMeetings.Add(meeting.Topic, meeting);
                }

                Console.WriteLine(meeting.ToString());
                sendInvite(meeting);
            }
            else
                Console.WriteLine("Couldn't create meeting!\nMeeting with topic " + topic + " already exists!");
        }

        public void createMeeting(String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            while (checkServerNeedsUpdate()) ;

            Meeting meeting = _serverService.createMeeting(_username, topic, minAtt, slots, invitees).Meeting;
            if (meeting != null) {
                lock (_clientMeetings) {
                    _clientMeetings.Add(meeting.Topic, meeting);
                }

                Console.WriteLine(meeting.ToString());
                sendInvite(meeting);
            }
            else
                Console.WriteLine("Couldn't create meeting!\nMeeting with topic " + topic + " already exists!");
        }

        public void joinMeetingSlot(String topic, String slot){
            while (checkServerNeedsUpdate()) { _serverService.updateServer(); };
            Meeting meeting = _serverService.getMeeting(topic);
            if (meeting == null) {
                Console.WriteLine("Couldn't join meeting!\nMeeting " + topic + " doesn't exist!");
                return;
            }
            if (!meeting.checkInvitation(_username)) {
                Console.WriteLine("You were not invited to this meeting!");
            }
            meeting = _serverService.joinMeetingSlot(topic, slot, _username).Meeting;
            if (meeting != null)
                Console.WriteLine(meeting.ToString());
            else
                Console.WriteLine("Couldn't join meeting!\nDesired slot not found!");
        }

        public void closeMeeting(String topic) {
            while (checkServerNeedsUpdate()) { _serverService.updateServer(); };
            Meeting meeting = _serverService.closeMeeting(topic, _username).Meeting;

            if (meeting == null)
                Console.WriteLine("Couldn't close meeting!\nMinimum number of Atendees not yet reached!");
        }

        public Meeting getMeeting(String topic) {
            while (checkServerNeedsUpdate()) { _serverService.updateServer(); };
            return _serverService.getMeeting(topic);
        }

        public void sendInvite(Meeting meeting) {
            _auxToInvites = new List<string>();
            getRegisteredClients();
           
            populateAuxInvite(meeting);
            Console.WriteLine("Sending invites...");
            foreach (String invitee in new List<string>(_auxToInvites)) {
                _otherClients[invitee].receiveInvite(meeting, _username);
            }
            
        }

        public void receiveInvite(Meeting meeting, String usernameSender) {
            getRegisteredClients();
            
            if (!_clientMeetings.ContainsKey(meeting.Topic)) {
                Console.WriteLine("[INVITE] Received an invitation to meeting " + meeting.Topic);
                lock (_clientMeetings) {
                    _clientMeetings.Add(meeting.Topic, meeting);
                }

                populateAuxInvite(meeting);
                _auxMeetingAlredySent = new List<string>();
                if (_auxToInvites.Count == 1 && _auxToInvites[0].Equals(usernameSender)) { //in the case that there is only a need to send the invite to one client and the chosen one was this.client
                    populateAuxInvite(meeting);
                }
                
                _otherClients[usernameSender].receiveInvite(meeting, _username); //resends the invitee in order for the other user to know we received
                if (_auxToInvites.Contains(usernameSender)) {                    //if the sender was a client on our aux list, we dont wanna resend again the invitee in the future
                    _auxToInvites.Remove(usernameSender);
                }
                foreach (String invitee in new List<string>(_auxToInvites)) {
                    _auxMeetingAlredySent.Add(usernameSender);
                    _otherClients[invitee].receiveInvite(meeting, _username);
                }
            }
            else {
                
                //If we no longer have any clients in our aux list then it means that 
                //all the clients we sent a message to, sent us one back and so, we no longer have to send them the invite again
                if (_auxToInvites != null && !_auxMeetingAlredySent.Contains(usernameSender)) {
                    _auxMeetingAlredySent.Add(usernameSender);    //this way, if the same usernameSender resends the invitee we no longer re-propagate it again
                    foreach (String invitee in new List<string>(_auxToInvites)) {         
                        _otherClients[invitee].receiveInvite(meeting, _username);
                    }
                    if (_auxToInvites.Contains(usernameSender)) { //only removed after sending the answer
                        _auxToInvites.Remove(usernameSender);
                    }
                }
            }            
        }

        public void populateAuxInvite(Meeting meeting) {
            if (meeting.Invitees == null) {                         //if there are no invitees then we want to send to every client 
                _auxToInvites = new List<string>(_otherClients.Keys);

            }
            else {
                _auxToInvites = new List<string>(meeting.Invitees);
            }

            if (_auxToInvites.Contains(_username)) {    //we dont want the client to send to himself
                _auxToInvites.Remove(_username);
            }

            int N = _auxToInvites.Count;
            if (N > 1) {                            //so, we divide the _otherClients list in two halfs and attribute a half to our clint via a random binary
                Random random = new Random();       //however we dont want every client to know every otherclient 
                Dictionary<int, string> auxList = new Dictionary<int, string>();
                int w = 0;
                while (w <= ((N/2))){ //half of all the clients -- in order to guarantee that every client receives at least one invite
                    int index = random.Next(_auxToInvites.Count);
                    if (!auxList.ContainsKey(index)) {
                        auxList.Add(index, _auxToInvites[index]);
                        w++;
                    }
                }
                if (auxList.Count != 0) {
                    _auxToInvites = new List<string>(auxList.Values);
                }
            }
        }

        public void wait(int time) {
            System.Threading.Thread.Sleep(time);
        }

        public String status() {
            String s = _username + " known Meetings: \n";
            foreach (Meeting meeting in _clientMeetings.Values) {
                s += meeting.status();
            }
            
            return s;
        }
    }
}
