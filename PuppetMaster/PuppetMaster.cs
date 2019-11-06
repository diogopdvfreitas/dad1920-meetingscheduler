﻿using System;
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

namespace PuppetMaster {
    public class PuppetMaster {

        private TcpChannel _channel;
        private Dictionary<String, IPCSService> _pcsList;     // <IP, IPCService>
        private String _scriptName = "testPM.txt";
        private List<Location> _locations;

        public PuppetMaster() {
            _channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(_channel, false);
            
            _pcsList = new Dictionary<String, IPCSService>();
            _locations = new List<Location>();

            PCSConfig();
        }

        public void PCSConfig() {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            foreach (String key in ConfigurationManager.AppSettings) {
                String pcsUrl = appSettings.Get(key);
                String[] urlAttributes = pcsUrl.Split(new Char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries);
                IPCSService pcServ = (IPCSService) Activator.GetObject(typeof(IPCSService), pcsUrl);
                _pcsList.Add(urlAttributes[1], pcServ);
                Console.WriteLine("[" + key + "] " + pcsUrl);
            }
        }

        public String Script {
            set { _scriptName = value; }
        }

        public void createServer(String[] commandAttr) {
            String[] server_url = commandAttr[2].Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);
            String server_IP = server_url[1];
            String server_port = server_url[2];

            //createServer: srvr_port, srvr_id, url, max_faults, min_delay, max_delay
           _pcsList[server_IP].createServer(server_port, commandAttr[1], commandAttr[2], commandAttr[3], commandAttr[4], commandAttr[5]);

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
            foreach (IPCSService pcservice in _pcsList.Values) {
                foreach (KeyValuePair<String, Process> processDct in pcservice.Processes) {
                    bool processResponding = processDct.Value.Responding;
                    String response = "";
                    if (pcservice.ServerURLs.ContainsKey(processDct.Key)) {
                        response += "Server " + processDct.Key;
                        if (processResponding) {
                            IServerService serverService = (IServerService)Activator.GetObject(typeof(IServerService), pcservice.ServerURLs[processDct.Key]);

                            response += " is present.\n";

                            Console.WriteLine(response);
                            serverService.printStatus();
                        }
                        else {
                            response += " has failed! \n";
                            Console.WriteLine(response);
                        }
                    }
                    else {
                        if (processResponding) {
                            ClientAPI client = (ClientAPI)Activator.GetObject(typeof(ClientAPI), pcservice.ClientURLs[processDct.Key]);

                            response += "Client " + processDct.Key + " is connected.\n";

                            Console.WriteLine(response);
                            client.printStatus();
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
                    //serverService.freeze();
                }
            }
        }

        public void unfreeze(String processId) {
            foreach (IPCSService pcservice in _pcsList.Values) {
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
            Console.WriteLine("Read Script " + _scriptName);
            StreamReader script;
            try {
                script = File.OpenText("../../../" + _scriptName);
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
            Console.WriteLine("|========== Meeting Scheduler PuppetMaster ==========|");
            PuppetMaster puppetMaster = new PuppetMaster();
            Console.Write("Please write the script filename: ");
            String scriptName = Console.ReadLine();
            puppetMaster.Script = scriptName;
            puppetMaster.readPuppetMasterScript();
            Console.ReadLine();
        }
    }
}