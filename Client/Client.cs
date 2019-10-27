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

        private TcpChannel _channel;
        private String _clientUrl = "tcp://localhost:8080/CLIENT"; //Estes url sao fornecido pelos PCS quando se cria o cliente
        private String _serverUrl = "tcp://localhost:8086/SERVER";
        private List<String> _otherClients;

        private ClientService _clientService;

        //serverService: interface to contact the server
        private IServerService _serverService;

        Hashtable _clientMeetings = new Hashtable();
        
        //Client: create a client with the defined urls
        public Client() {
            connectServer();
        }

        //Client: create a client with the given urls
        public Client(String clientUrl, String serverUrl) {
            _clientUrl = clientUrl;
            _serverUrl = serverUrl;
            connectServer();
        }

        //connectServer: connect to the server, save the ref to the server remote obj and asks for registered clients
        //???if there´s the necessity to connect to more than on server in each session the server url should be an argument of the function
        public void connectServer() {
            _channel = new TcpChannel();
            ChannelServices.RegisterChannel(_channel, false);

            _clientService = new ClientService();
            RemotingServices.Marshal(_clientService, "CLIENT", typeof(ClientService));

            _serverService = (IServerService) Activator.GetObject( typeof(IServerService), _serverUrl);
            _serverService.connect(_clientUrl);
            getRegisteredClients();
            
        }

        //getRegisteredClients: ask the server for registered clients
        public void getRegisteredClients() {
            _otherClients = _serverService.getRegisteredClients();
            foreach (String s in _otherClients)
                Console.WriteLine("Clients: " + s);
        }

        //listMeeting: the server contact the server to know about the status of the meetings he already knows
        //?????check if there´s a beter way to do this
        public void listMeetings() {
            Console.WriteLine("|========== Meetings ==========|");

            String list = "";
            List<String> meetingStatusChanged = new List<String>();

            // Checks if Meeting Status is still the same as the server's
            foreach (DictionaryEntry m in _clientMeetings) {
                if (_serverService.checkMeetingStatusChange((Meeting)(m.Value)))
                    meetingStatusChanged.Add(((Meeting)(m.Value)).Topic);
            }

            // If not updates it
            foreach (String topic in meetingStatusChanged) {
                _clientMeetings[topic] = _serverService.getMeeting(topic);
            }

            foreach (DictionaryEntry m in _clientMeetings) {
                list += ((Meeting)(m.Value)).ToString(); 
            }
            Console.WriteLine(list);
           
        }

        public void createMeeting(String topic, int minAtt, List<Slot> slots) {
            Meeting meeting = new Meeting(_clientUrl, topic, minAtt, slots);
            _clientMeetings.Add(topic, meeting);
            _serverService.createMeeting(meeting);

            Console.WriteLine("Meeting Created");
        }

        public void createMeeting(String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            Meeting meeting = new Meeting(_clientUrl, topic, minAtt, slots, invitees);
            _clientMeetings.Add(topic, meeting);
            _serverService.createMeeting(meeting);

            Console.WriteLine("Meeting Created");
        }
    }
}
