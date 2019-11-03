using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using CreationServiceLibrary;
using System.Configuration;
using System.IO;
using RemotingServicesLibrary;
using System.Diagnostics;

namespace PuppetMaster {
    public class PuppetMaster {

        private TcpChannel _channel;
        private Dictionary<String, IPCSService> pcsList;     // <IP, IPCService>
        private String _scriptName = "testPM.txt";

        public PuppetMaster(String scriptName) {
            _scriptName = scriptName;
            _channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(_channel, false);
            

            pcsList = new Dictionary<String, IPCSService>();
            PCSConfig();
        }

        public void PCSConfig() {
            foreach (String pcsUrl in ConfigurationManager.AppSettings) {
                String[] urlAttributes = pcsUrl.Split(new Char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries);
                Console.WriteLine(pcsUrl);
                IPCSService pcServ = (IPCSService) Activator.GetObject(typeof(IPCSService), pcsUrl);
                pcsList.Add(urlAttributes[1], pcServ);
            }
        }

        public void createServer(String[] commandAttr) {
            String[] server_url = commandAttr[2].Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);
            String server_IP = server_url[1];
            String server_port = server_url[2];

            //createServer: srvr_port, srvr_id, url, max_faults, min_delay, max_delay
            pcsList[server_IP].createServer(server_port, commandAttr[1], commandAttr[2], commandAttr[3], commandAttr[4], commandAttr[5]);

        }

        //createClient: contact the PCS with the client ip in order to create the pretend client
        public void createClient(String[] commandAttr) {
            String[] clientUrl = commandAttr[2].Split(new Char[] { '/', ':'}, StringSplitOptions.RemoveEmptyEntries);
            String clientIp = clientUrl[1];
            //createClient, args: username, clientUrl, serverUrl, scriptFile
            pcsList[clientIp].createClient(commandAttr[1], commandAttr[2], commandAttr[3], commandAttr[4]);
        }

        public void addRoom(String[] commandAttr) { // we implemented this considering that there is already a server created, at least
            foreach(IPCSService pcservice in pcsList.Values) {
                foreach(String serverURL in pcservice.ServerURLs.Values) {
                    IServerService serverService = (IServerService)Activator.GetObject(typeof(IServerService), serverURL);
                    serverService.addRoom(commandAttr[1], Int32.Parse(commandAttr[2]), commandAttr[3]); // location, capacity, room_name
                }
            }
        }

        public void status() {
            foreach (IPCSService pcservice in pcsList.Values) {
                foreach (KeyValuePair<String, Process> processDct in pcservice.Processes) {
                    bool processResponding = processDct.Value.Responding;
                    String response = "";
                    if (pcservice.ServerURLs.ContainsKey(processDct.Key)) {
                        response += "Server " + processDct.Key;
                        if (processResponding)
                            response += " is present.";
                        else
                            response += " has failed!";
                    }
                    else {
                        if (processResponding)
                            response += "Client " + processDct.Key + " is connected.";
                    }
                    Console.WriteLine(response);
                }
            }
        }

        public void crash(String processId) {
            foreach (IPCSService pcservice in pcsList.Values) {
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
            foreach (IPCSService pcservice in pcsList.Values) {
                if (pcservice.ServerURLs.ContainsKey(processId)) {
                    IServerService serverService = (IServerService)Activator.GetObject(typeof(IServerService), pcservice.ServerURLs[processId]);
                    //serverService.freeze();
                }
            }
        }

        public void unfreeze(String processId) {
            foreach (IPCSService pcservice in pcsList.Values) {
                if (pcservice.ServerURLs.ContainsKey(processId)) {
                    IServerService serverService = (IServerService)Activator.GetObject(typeof(IServerService), pcservice.ServerURLs[processId]);
                    //serverService.unfreeze();
                }
            }
        }

        public void wait(String miliseconds) {
            System.Threading.Thread.Sleep(int.Parse(miliseconds));
        }

        public void executeCommand(String command) {
            Console.WriteLine("[COMMAND] " + command);
            String[] commandAttr = command.Split(' ');

            switch (commandAttr[0]) {
                case "Server":
                    createServer(commandAttr);
                    break;

                case "Client":
                    createClient(commandAttr);
                    break;

                case "AddRoom":
                    addRoom(commandAttr);
                    break;

                case "Status":
                    status();
                    break;

                case "Crash":
                    crash(commandAttr[1]);
                    break;

                case "Freeze"://TODO
                    freeze(commandAttr[1]);
                    break;

                case "Unfreeze"://TODO
                    break;
                case "Wait":
                    wait(commandAttr[1]);
                    break;
            }
        }

        public void readPuppetMasterScript() {
            Console.WriteLine("Read Script");
            StreamReader script;
            try {
                script = File.OpenText(_scriptName);
            }
            catch (FileNotFoundException) {
                Console.WriteLine("File: " + _scriptName + " Not Found");
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
            Console.WriteLine("Meeting Scheduler PuppetMaster");
            String scriptName = "../../../" + Console.ReadLine();
            Console.WriteLine("ScriptFile: " + scriptName );
            PuppetMaster puppetMaster = new PuppetMaster(scriptName);
            puppetMaster.readPuppetMasterScript();
            Console.ReadLine();
        }
    }
}