using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using ObjectsLibrary;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using RemotingServicesLibrary;
using System.Runtime.Remoting;

namespace Client {
    public class Client {

        private String _scriptName = "test";

        private TcpChannel _channel;
        private String _clientUrl = "tcp://localhost:8080/CLIENT"; //Estes url e fornecido pelos PCS quando se cria o cliente
        private String _serverUrl = "tcp://localhost:8086/SERVER";

        private ClientService clientService;

        //serverService: interface to contact the server
        private IServerService serverService;

        Hashtable clientMeetings = new Hashtable();


        //create a client from a scrip file
        public Client(String clientUrl, String serverUrl, String scriptName) {
            _clientUrl = clientUrl;
            _serverUrl = serverUrl;
            _scriptName = scriptName;
            connectServer();
        }

        public Client() {
            connectServer();
        }

        //connectServer: we connect to the server given when creating the client and we save the ref to the server remote obj
        //if there´s the necessity to connect to more than on server in each session the server url should be an argument of the function
        public void connectServer() {
            _channel = new TcpChannel();
            ChannelServices.RegisterChannel(_channel, false);

            clientService = new ClientService();
            RemotingServices.Marshal(clientService, "CLIENT", typeof(ClientService));

            serverService = (IServerService) Activator.GetObject( typeof(IServerService), _serverUrl);
        }

        
        //listMeeting: the server contact the server to know about the status of the meetings he already knows
    //?????check if there´s a beter way to do this
        public void listMeeting() {
            Console.WriteLine("ListMeeting");

            String list = "";

            List<String> changedStatusMt = new List<String>();

            foreach (DictionaryEntry m in clientMeetings) {
                if (serverService.checkMeetingStatus((Meeting)(m.Value)))
                    changedStatusMt.Add(((Meeting)(m.Value)).Topic);
            }
            foreach (String s in changedStatusMt) {
                clientMeetings[s] = serverService.getMeeting(s);
            }

            foreach (DictionaryEntry m in clientMeetings) {
                list += ((Meeting)(m.Value)).print() + " "; 
            }
            Console.WriteLine(list);
           
        }

        public void createMeeting(Meeting meeting) {
            clientMeetings.Add(meeting.Topic, meeting);
            serverService.createMeeting(meeting);

            Console.WriteLine("CreateMeetingEnd");
        }



        public void readClientScript() {
            Console.WriteLine("Read Script");
            StreamReader script;
            try {
                script = File.OpenText(_scriptName);

            } catch (FileNotFoundException) {
                Console.WriteLine("File Not Found");
                return;
            }

            String scriptLine = script.ReadLine();
            while (scriptLine != null) {
                Console.WriteLine("Linha lida: " + scriptLine);
                executeCommand(scriptLine);
                Console.WriteLine("ProxCommando");
                scriptLine = script.ReadLine();
            }
            Console.WriteLine("ProxCommando");
            script.Close();
        }

        public void executeCommand(String command) {
            String[] commandAttr = command.Split(' ');

            List<String> slots;
            List<String> invits;
            int nSlots;
            int nInvits;
            int limit1;
            int limit2;

            switch (commandAttr[0].ToString()) {
                //TODO 
                case "list":
                    listMeeting();
                    break;

                case "create":
                    nSlots = Int32.Parse(commandAttr[3].ToString());
                    nInvits = Int32.Parse(commandAttr[4].ToString());

                    slots = new List<String>();  //if we have the class slot this should be a list of slots instead of a list of Strings
                    invits = new List<String>();

                    //select the slots from the command
                    limit1 = 5 + nSlots;
                    for (int i = 5; i < limit1; i++) {
                        slots.Add(commandAttr[i].ToString());
                    }

                    //select the invitees from the command
                    limit2 = limit1 + nInvits;
                    for (int i = limit1; i < limit2; i++) {
                        invits.Add(commandAttr[i].ToString());
                    }

                    createMeeting(new Meeting(_clientUrl, commandAttr[1].ToString(), Int32.Parse(commandAttr[2].ToString()), nSlots, nInvits, slots, invits));
                   break;

                case "join":
                    nSlots = Int32.Parse(commandAttr[2].ToString());
                    limit1 = 3 + nSlots;
                    Console.WriteLine("Join Meeting");
                    break;

                case "close":
                    Console.WriteLine("Close Meeting");
                    break;

                case "wait":
                    int time = Int32.Parse(commandAttr[1].ToString());
                    Console.WriteLine("Wait");
                    break;
            }
        }

        static void Main(string[] args) {
            Console.WriteLine("Ola");
            Console.ReadLine();
            Client client = new Client();
            client.readClientScript();

            Console.ReadLine();
        }


        }
    }
