using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using CreationServiceLibrary;
using System.Configuration;
using System.IO;
using RemotingServicesLibrary;
using System.Diagnostics;
using System.Collections.Specialized;
using ClientLibrary;
using ObjectsLibrary;
using System.Net.Sockets;

namespace PuppetMaster {
    public class PuppetMaster {

        private TcpChannel _channel;
        private Dictionary<String, IPCSService> _pcsList;     // <IP, IPCService>
        private String _scriptName = "testPM.txt";
        private List<Location> _locations;

        public delegate void createServerDelegate(String[] commandAttr);
        public delegate void createClientDelegate(String[] commandAttr);
        public delegate void addRoomDelegate(String[] commandAttr);
        public delegate void statusDelegate();
        public delegate void crashDelegate(String commandAttr1);
        public delegate void freezeDelegate(String commandAttr1);
        public delegate void unfreezeDelegate(String commandAttr1);


        public PuppetMaster() {
            _channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(_channel, false);
            
            _pcsList = new Dictionary<String, IPCSService>();
            _locations = new List<Location>();

            PCSConfig();
        }

        public void PCSConfig() {
            Console.WriteLine("|============================== PCS' List ==============================|");
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            foreach (String key in ConfigurationManager.AppSettings) {
                String pcsUrl = appSettings.Get(key);
                String[] urlAttributes = pcsUrl.Split(new Char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries);
                IPCSService pcServ = (IPCSService) Activator.GetObject(typeof(IPCSService), pcsUrl);
                _pcsList.Add(urlAttributes[1], pcServ);
                Console.WriteLine("[" + key + "] " + pcsUrl);
            }
            Console.WriteLine();
        }

        public String Script {
            set { _scriptName = value; }
        }

        public void createServer(String[] commandAttr) {
            String[] server_url = commandAttr[2].Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);
            String server_IP = server_url[1];
            String server_port = server_url[2];
            String server_obj = server_url[3];

            //createServer: srvr_port, srvr_id, url, max_faults, min_delay, max_delay, server_obj
           _pcsList[server_IP].createServer(server_port, commandAttr[1], commandAttr[2], commandAttr[3], commandAttr[4], commandAttr[5], server_obj);
            if(_locations != null) {
                IServerService serverService = (IServerService)Activator.GetObject(typeof(IServerService), _pcsList[server_IP].ServerURLs[commandAttr[1]]);
                foreach (Location location in _locations) {
                    serverService.addLocation(location.Name, location);
                }
            }
        }

        //createClient: contact the PCS with the client ip in order to create the pretend client
        public void createClient(String[] commandAttr) {
            String[] clientUrl = commandAttr[2].Split(new Char[] { '/', ':'}, StringSplitOptions.RemoveEmptyEntries);
            String clientIp = clientUrl[1];
 
            //createClient, args: username, clientUrl, serverUrl, scriptFile
            _pcsList[clientIp].createClient(commandAttr[1], commandAttr[2], commandAttr[3], commandAttr[4]);
        }

        public void addRoom(String[] commandAttr) {
            Location location = new Location(commandAttr[1]);
            location.addRoom(new Room(commandAttr[3], Int32.Parse(commandAttr[2]))); // room_name, capacity
            _locations.Add(location);
        }

        public void status() {
            Console.WriteLine("|================================ STATUS ================================|");

            var e = _pcsList.GetEnumerator();
            e.MoveNext();
            var pcservice = e.Current.Value;
            foreach (KeyValuePair<String, Process> processDct in pcservice.Processes) {
                bool processResponding;
                try {
                    processResponding = processDct.Value.Responding;
                }
                catch (InvalidOperationException) {
                    processResponding = false;
                }
                String response = "";
                if (pcservice.ServerURLs.ContainsKey(processDct.Key)) {
                    response += "[SERVER:" + processDct.Key + "]";
                    if (processResponding) {
                        IServerService serverService = (IServerService)Activator.GetObject(typeof(IServerService), pcservice.ServerURLs[processDct.Key]);

                        try {
                            String state = " Present";
                            state += serverService.status();
                            response += state;
                            Console.WriteLine(response);

                        }
                        catch (SocketException) {
                            response += " has failed! \n";
                            Console.WriteLine(response);

                        }
                    }
                    else {
                            response += " has failed! \n";
                            Console.WriteLine(response);
                    }
                }
                else {
                    response += "[CLIENT:" + processDct.Key + "]";
                    if (processResponding) {
                        IClientService client = (IClientService)Activator.GetObject(typeof(IClientService), pcservice.ClientURLs[processDct.Key]);

                        try {
                            String state = "Connected";
                            state += client.status();
                            response += state;
                            Console.WriteLine(response);

                        }
                        catch (SocketException) {
                            response += "has failed!\n";
                            Console.WriteLine(response);
                        }
                    }
                }       
            }
        }

        public void crash(String processId) {
            foreach (IPCSService pcservice in _pcsList.Values) {
                pcservice.Processes[processId].Kill();
                pcservice.Processes.Remove(processId);
                if (pcservice.ServerURLs.ContainsKey(processId)) {
                    pcservice.ServerURLs.Remove(processId);
                }
                else {
                    pcservice.ClientURLs.Remove(processId);
                }
            }
        }

        public void freeze(String processId) {
            foreach (IPCSService pcservice in _pcsList.Values) {
                if (pcservice.ServerURLs.ContainsKey(processId)) {
                    IServerService serverService = (IServerService)Activator.GetObject(typeof(IServerService), pcservice.ServerURLs[processId]);
                    serverService.freeze();
                }
            }
        }

        public void unfreeze(String processId) {
            foreach (IPCSService pcservice in _pcsList.Values) {
                if (pcservice.ServerURLs.ContainsKey(processId)) {
                    IServerService serverService = (IServerService)Activator.GetObject(typeof(IServerService), pcservice.ServerURLs[processId]);
                    serverService.unfreeze();
                }
            }
        }

        public void wait(String miliseconds) {
            System.Threading.Thread.Sleep(int.Parse(miliseconds));
        }

        public void shutDown(){ 
            foreach(IPCSService pcs in _pcsList.Values){
                pcs.shutDown();
            }
        }

        public void executeCommand(String command) {
            Console.WriteLine("[COMMAND] " + command);
            String[] commandAttr = command.Split(' ');

            switch (commandAttr[0]) {
                case "Server":
                    createServerDelegate createServer_Del = new createServerDelegate(createServer);
                    createServer_Del.BeginInvoke(commandAttr, null, null);
                    break;

                case "Client":
                    createClientDelegate createClient_Del = new createClientDelegate(createClient);
                    createClient_Del.BeginInvoke(commandAttr, null, null);
                    break;

                case "AddRoom":
                    addRoomDelegate addRoom_Del = new addRoomDelegate(addRoom);
                    addRoom_Del.BeginInvoke(commandAttr, null, null);
                    break;

                case "Status":
                    statusDelegate status_Del = new statusDelegate(status);
                    status_Del.BeginInvoke(null, null);
                    break;

                case "Crash":
                    crashDelegate crash_Del = new crashDelegate(crash);
                    crash_Del.BeginInvoke(commandAttr[1], null, null);
                    break;

                case "Freeze"://TODO
                    freezeDelegate freeze_Del = new freezeDelegate(freeze);
                    freeze_Del.BeginInvoke(commandAttr[1], null, null);
                    break;

                case "Unfreeze"://TODO
                    unfreezeDelegate unfreeze_Del = new unfreezeDelegate(unfreeze);
                    unfreeze_Del.BeginInvoke(commandAttr[1], null, null);
                    break;
                case "Wait":
                    wait(commandAttr[1]);
                    break;
            }
        }

        public void readPuppetMasterScript() {
            Console.WriteLine("Reading Script " + _scriptName + "\n");
            StreamReader script;
            try {
                script = File.OpenText("../../../" + _scriptName);
            }
            catch (FileNotFoundException) {
                Console.WriteLine("File: " + _scriptName + " Not Found");
                Console.Write("[Write the correct script filename]");
                String scriptName = Console.ReadLine();
                this.Script = scriptName;
                this.readPuppetMasterScript();
                return;
            }

            String scriptLine = script.ReadLine();
            while (scriptLine != null) {
                executeCommand(scriptLine);
                scriptLine = script.ReadLine();
            }
            script.Close();
        }

        static void Main(string[] args) {
            Console.WriteLine("+========================================================================+\n" +
                              "|==================== Meeting Scheduler PuppetMaster ====================|\n" +
                              "+========================================================================+\n");
            PuppetMaster puppetMaster = new PuppetMaster();
            Console.Write("[Write the script filename]");
            String scriptName = Console.ReadLine();
            puppetMaster.Script = scriptName;
            puppetMaster.readPuppetMasterScript();

            Console.WriteLine("[COMAND OPTIONS]");
            Console.WriteLine("\t[SHUTDOWN to Kill all Processes]");
            Console.WriteLine("\t[QUIT to Exit]");

            while (true) {
                Console.Write("[COMMAND]");
                String command = Console.ReadLine();
                if (command.Equals("SHUTDOWN") || command.Equals("shutdown")) {
                    puppetMaster.shutDown();
                    break;
                }
                else if (command.Equals("QUIT") || command.Equals("quit"))
                    break;
            }
        }
    }
}