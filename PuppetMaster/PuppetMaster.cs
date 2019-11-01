using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using CreationServiceLibrary;
using System.Configuration;
using System.IO;

namespace PuppetMaster {
    public class PuppetMaster {

        private TcpChannel _channel;
        private Dictionary<String, IPCService> pcsList;     // <ip, IPCService>
        private String _scriptName = "testPM.txt";

        public PuppetMaster(String scriptName) {
            _scriptName = scriptName;
            _channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(_channel, false);

            pcsList = new Dictionary<String, IPCService>();
        }

        public void PCSConfig() {
            foreach (String pcsUrl in ConfigurationManager.AppSettings) {
                String[] urlAttributes = pcsUrl.Split(new Char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries);
                IPCService pcServ = (IPCService) Activator.GetObject(typeof(IPCService), "PCSERVICE", pcsUrl);
                pcsList.Add(urlAttributes[1], pcServ);
            }
        }

        public void createServer(String[] commandAttr) {
            String[] server_url = commandAttr[2].Split(new char[] { '/', ':' }, StringSplitOptions.RemoveEmptyEntries);
            String server_IP = server_url[1];
            String server_port = server_url[2];

            //createServer: srvr_id, url, min_delay, max_delay
            pcsList[server_IP].createServer(server_port, commandAttr[1], commandAttr[2], commandAttr[3], commandAttr[4], commandAttr[5]);

        }

        //createClient: contact the PCS with the client ip in order to create the pretend client
        public void createClient(String[] commandAttr) {
            String[] clientUrl = commandAttr[2].Split(new Char[] { '/', ':'}, StringSplitOptions.RemoveEmptyEntries);
            String clientIp = clientUrl[1];
            //createClient, args: username, clientUrl, serverUrl, scriptFile
            pcsList[clientIp].createClient(commandAttr[1], commandAttr[2], commandAttr[3], commandAttr[4]);
        }


        public void executeCommand(String command) {
            String[] commandAttr = command.Split(' ');

            switch (commandAttr[0]) {
                case "Server":
                    createServer(commandAttr);

                    break;
                case "Client":
                    createClient(commandAttr);


                    break;
                case "AddRoom":
                    break;
                case "Status":
                    break;
                case "Crash":
                    break;
                case "Freeze":
                    break;
                case "Unfreeze":
                    break;
                case "Wait":
                    break;
            }
        }

        public void readClientScript() {
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
            String scriptName= Console.ReadLine();
            PuppetMaster puppetMaster = new PuppetMaster(scriptName);

        }
    }
}
