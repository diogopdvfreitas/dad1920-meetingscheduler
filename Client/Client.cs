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
        private String _username = "Rita";
        private String _url = "tcp://localhost:8080/CLIENT";
        private int _port = 8080;
        private String _serverUrl = "tcp://localhost:8086/SERVER";

        private TcpChannel _channel;

        private ClientService _clientService;
        private IServerService _serverService;

        private IDictionary<String, Meeting> _clientMeetings;           // <Meeting Topic, Meeting>
        private IDictionary<String, IClientService> _otherClients;      // <Client 

        public Client() {
            Console.WriteLine("Client " + _username + " at " + _url);

            _clientMeetings = new Dictionary<String, Meeting>();
            _otherClients = new Dictionary<String, IClientService>();
            setClient();
            connectToServer();
        }

        public Client(String username, String clientUrl, String serverUrl) {
            _url = clientUrl;
            String[] clientUrlSplit = clientUrl.Split(new Char[] { ':', '/'}, StringSplitOptions.RemoveEmptyEntries);
            _port = Int32.Parse(clientUrlSplit[2]);
            _serverUrl = serverUrl;
            _username = username;
            Console.WriteLine("Client " + _username + " at " + _url);

            _clientMeetings = new Dictionary<String, Meeting>();
            _otherClients = new Dictionary<String, IClientService>();
            setClient();
            connectToServer();
        }

        public void setClient() {
            _channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(_channel, false);

            _clientService = new ClientService(this);
            RemotingServices.Marshal(_clientService, "CLIENT", typeof(ClientService));

            Console.WriteLine("Client " + _username + " created at port " + _port);
        }

        // connectServer: connect to the server, save the ref to the server remote obj and asks for registered clients
        // ???if there´s the necessity to connect to more than on server in each session the server url should be an argument of the function
        public void connectToServer() {
            _serverService = (IServerService) Activator.GetObject( typeof(IServerService), _serverUrl);

            getRegisteredClients();

            _serverService.clientConnect(_username, _url);
        }

        public void getRegisteredClients() {
            IDictionary<String, String> registeredClients = _serverService.getRegisteredClients();
            foreach (KeyValuePair<String, String> client in registeredClients) {
                if (client.Key != _username) {
                    IClientService clientServ = (IClientService)Activator.GetObject(typeof(IClientService), client.Value);
                    _otherClients.Add(client.Key, clientServ);
                    Console.WriteLine("Registered Clients: username " + client.Key + " url: " + client.Value);
                }
            }
        }

        public void listMeetings() {
            Console.WriteLine("|========== Meetings ==========|");

            String list = "";
            List<String> meetingStatusChanged = new List<String>();

            foreach (Meeting meeting in _clientMeetings.Values) {
                if (_serverService.checkMeetingStatusChange(meeting)) {
                    meetingStatusChanged.Add(meeting.Topic);
                }
            }

            foreach (String topic in meetingStatusChanged) {
                _clientMeetings[topic] = _serverService.getMeeting(topic);
            }

            foreach (Meeting meeting in _clientMeetings.Values) {
                list += meeting.ToString(); 
            }
            Console.WriteLine(list);  
        }

        public void createMeeting(String topic, int minAtt, List<Slot> slots) {
            
            Meeting meeting = _serverService.createMeeting(_username, topic, minAtt, slots);
            _clientMeetings.Add(meeting.Topic, meeting);

            Console.WriteLine("Meeting " + topic + " created");

            sendInvite(meeting);
        }

        public void createMeeting(String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            Meeting meeting = _serverService.createMeeting(_username, topic, minAtt, slots, invitees);
            _clientMeetings.Add(meeting.Topic, meeting);

            Console.WriteLine("Meeting " + topic + " Created");

            sendInvite(meeting);
        }

        public Meeting getMeeting(String topic) {
            return _serverService.getMeeting(topic);
        }

        public void joinMeetingSlot(String topic, Slot chosenSlot){
            _serverService.joinMeetingSlot(topic, chosenSlot, _username);
            Console.WriteLine(_username + " joined meeting " + topic + " on slot " + chosenSlot);
        }

        public void closeMeeting(String topic) {
            _serverService.closeMeeting(topic);
        }

        public void sendInvite(Meeting meeting) {
            getRegisteredClients();
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

        public void receiveInvite(Meeting meeting) {
            _clientMeetings.Add(meeting.Topic, meeting);
        }
    }
}
