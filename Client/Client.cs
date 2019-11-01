using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using ObjectsLibrary;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using RemotingServicesLibrary;
using System.Runtime.Remoting;
using ClientLibrary;

namespace Client {
    public class Client : ClientAPI {

        //default values
        private String _username = "Pedro";
        private String _clientUrl = "tcp://localhost:8080/CLIENT";
       
        private String _serverUrl = "tcp://localhost:8086/SERVER";

        private TcpChannel _channel;
        private ClientService _clientService;

        //serverService: interface to contact the server
        private IServerService _serverService;

        protected IDictionary<String, Meeting> _clientMeetings; //<topic, meeting>
        private Dictionary<String,IClientService> _otherClients;


        //Client: create a client with the defined urls
        public Client() {
            _clientMeetings = new Dictionary<String, Meeting>();
            _otherClients = new Dictionary<String, IClientService>();
            connectServer();
        }

        //Client: create a client with the given username and urls
        public Client(String username, String clientUrl, String serverUrl) {
            _clientUrl = clientUrl;
            _serverUrl = serverUrl;
            _username = username;
            _clientMeetings = new Dictionary<String, Meeting>();
            _otherClients = new Dictionary<String, IClientService>();
            connectServer();
        }

        //connectServer: connect to the server, save the ref to the server remote obj and asks for registered clients
        //???if there´s the necessity to connect to more than on server in each session the server url should be an argument of the function
        public void connectServer() {
            _channel = new TcpChannel();
            ChannelServices.RegisterChannel(_channel, false);

            _clientService = new ClientService(this);
            RemotingServices.Marshal(_clientService, "CLIENT", typeof(ClientService));

            _serverService = (IServerService) Activator.GetObject( typeof(IServerService), _serverUrl);

            getRegisteredClients();

            _serverService.connect(_username, _clientUrl);
        }

        //getRegisteredClients: ask the server for registered clients
        public void getRegisteredClients() {
            Dictionary<String, String> registeredClients = _serverService.getRegisteredClients();
            foreach (KeyValuePair<String, String> client in registeredClients) {
                IClientService clientServ = (IClientService)Activator.GetObject(typeof(IClientService), client.Value);
                _otherClients.Add(client.Key, clientServ);
                Console.WriteLine("Registered Clients: username " + client.Key + " url: " + client.Value);
            }
        }

        //listMeeting: list meetings known by the client
        public void listMeetings() {
            Console.WriteLine("|========== Meetings ==========|");

            String list = "";
            List<String> meetingStatusChanged = new List<String>();

            // Checks if Meeting Status is still the same as the server's
            foreach (Meeting meeting in _clientMeetings.Values) {
                if (_serverService.checkMeetingStatusChange(meeting)) {
                    meetingStatusChanged.Add(meeting.Topic);
                }
            }

            // If not updates it
            foreach (String topic in meetingStatusChanged) {
                _clientMeetings[topic] = _serverService.getMeeting(topic);
            }

            foreach (Meeting meeting in _clientMeetings.Values) {
                list += meeting.ToString(); 
            }
            Console.WriteLine(list);  
        }

        //getMeeting: return meeting with the given topic
        public Meeting getMeeting(String topic) {
            return _serverService.getMeeting(topic);
        }

        //createMeeting: create meeting without invitees
        public void createMeeting(String topic, int minAtt, List<Slot> slots) {
            Meeting meeting = new Meeting(_username, topic, minAtt, slots);
            _clientMeetings.Add(topic, meeting);
            _serverService.createMeeting(meeting);

            Console.WriteLine("Meeting " + topic + " Created");

            sendInvite(meeting);
        }

        //createMeeting: create meeting with invitees
        public void createMeeting(String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            Meeting meeting = new Meeting(_username, topic, minAtt, slots, invitees);
            _clientMeetings.Add(topic, meeting);
            _serverService.createMeeting(meeting);

            Console.WriteLine("Meeting " + topic + " Created");

            sendInvite(meeting);
        }

        //sendInvite: send meeting to  all clients or just to the inviteed clients
        public void sendInvite(Meeting meeting) {
            if (meeting.Invitees == null) {
                foreach (IClientService clientServ in _otherClients.Values)
                    clientServ.receiveInvite(meeting);
            }
            else {
                foreach (String invitee in meeting.Invitees) {
                    if (_otherClients.ContainsKey(invitee))
                        _otherClients[invitee].receiveInvite(meeting);
                    else
                        Console.WriteLine(invitee + " is not registered in the system.");
                }
            }            
        }

        //receiveInvite: receive meeting invitee
        public void receiveInvite(Meeting meeting) {
            _clientMeetings.Add(meeting.Topic, meeting);
        }

        //joinMeetingSlot: join meeting, with the given topic, in the given slot
        public void joinMeetingSlot(String topic, Slot chosenSlot){
            _serverService.joinMeetingSlot(topic, chosenSlot, _username);
            Console.WriteLine(_username + " joined meeting " + topic + " on slot " + chosenSlot);
        }

        //closeMeeting: close meeting with the given topic
        public void closeMeeting(String topic) {
            _serverService.closeMeeting(topic);
        }
    }
}
