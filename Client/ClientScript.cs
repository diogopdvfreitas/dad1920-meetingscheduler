using System;
using System.Collections.Generic;
using System.IO;
using ObjectsLibrary;

namespace Client {
    public class ClientScript {
        private String _scriptName = "test"; //"test" is just a test file
        Client client;

        public ClientScript() {
            client = new Client();
        }

        public void readClientScript() {
            Console.WriteLine("Read Script");
            StreamReader script;
            try {
                script = File.OpenText(_scriptName);
            }
            catch (FileNotFoundException) {
                Console.WriteLine("File: " + _scriptName +  " Not Found");
                return;
            }

            String scriptLine = script.ReadLine();
            while (scriptLine != null) {
                executeCommand(scriptLine);
                scriptLine = script.ReadLine();
            }
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
                    client.listMeeting();
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

                    client.createMeeting(commandAttr[1].ToString(), Int32.Parse(commandAttr[2].ToString()), nSlots, nInvits, slots, invits);
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
            ClientScript clientScript = new ClientScript();
            clientScript.readClientScript();

            Console.ReadLine();
        }
    }
}
