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

        private ClientService _clientService;

        //serverService: interface to contact the server
        private IServerService serverService;

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

        //connectServer: we connect to the server given when creating the client and we save the ref to the server remote obj
//???if there´s the necessity to connect to more than on server in each session the server url should be an argument of the function
        public void connectServer() {
            _channel = new TcpChannel();
            ChannelServices.RegisterChannel(_channel, false);

            _clientService = new ClientService();
            RemotingServices.Marshal(_clientService, "CLIENT", typeof(ClientService));

            serverService = (IServerService) Activator.GetObject( typeof(IServerService), _serverUrl);
        }

        
        //listMeeting: the server contact the server to know about the status of the meetings he already knows
 //?????check if there´s a beter way to do this
        public void listMeeting() {
            Console.WriteLine("ListMeeting");

            String list = "";

            List<String> changedStatusMt = new List<String>();

            foreach (DictionaryEntry m in _clientMeetings) {
                if (serverService.checkMeetingStatus((Meeting)(m.Value)))
                    changedStatusMt.Add(((Meeting)(m.Value)).Topic);
            }
            foreach (String s in changedStatusMt) {
                _clientMeetings[s] = serverService.getMeeting(s);
            }

            foreach (DictionaryEntry m in _clientMeetings) {
                list += ((Meeting)(m.Value)).print() + " "; 
            }
            Console.WriteLine(list);
           
        }

        public void createMeeting(String topic, int minAtt, int nSlots, int nInvits, List<String> slots, List<String> invits) {
            Meeting meeting = new Meeting(_clientUrl, topic, minAtt, nSlots, nInvits, slots, invits);
            _clientMeetings.Add(topic, meeting);
            serverService.createMeeting(meeting);

            Console.WriteLine("Meeting Created");
        }




        }
    }
