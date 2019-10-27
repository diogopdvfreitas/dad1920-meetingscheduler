using System;
using System.Collections.Generic;
using System.IO;
using ObjectsLibrary;

namespace Client {
    public class ClientScript {
        private String _scriptName = "test.txt";
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

            List<Slot> slots;
            int nSlots;
            int slotsStartIndex;

            switch (commandAttr[0]) {
                //TODO 
                case "list":
                    client.listMeetings();
                    break;

                case "create":
                    List<String> invitees;
                    int nInvitees;
                    int invtsStartIndex;

                    slots = new List<Slot>();
                    nSlots = Int32.Parse(commandAttr[3]);

                    // Select the slots from the command
                    slotsStartIndex = 5 + nSlots;
                    for (int i = 5; i < slotsStartIndex; i++) {
                        String[] slotAttr = commandAttr[i].Split(',');
                        Slot slot = new Slot(new Location(slotAttr[0]), slotAttr[1]);
                        slots.Add(slot);
                    }

                    // Invitees are optional
                    try {
                        invitees = new List<String>();
                        nInvitees = Int32.Parse(commandAttr[4].ToString());

                        // Select the invitees from the command
                        invtsStartIndex = slotsStartIndex + nInvitees;
                        for (int i = slotsStartIndex; i < invtsStartIndex; i++) {
                            invitees.Add(commandAttr[i]);
                        }
                        client.createMeeting(commandAttr[1], Int32.Parse(commandAttr[2]), slots, invitees);
                    }
                    catch (FormatException) {
                        nInvitees = 0;
                        client.createMeeting(commandAttr[1], Int32.Parse(commandAttr[2]), slots);
                    }
                    break;

                case "join":
                    Console.WriteLine("Join Meeting");

                    nSlots = Int32.Parse(commandAttr[2]);

                    for( int i = 3; i < 3 + nSlots; i++) {
                        String[] slotAttr = commandAttr[i].Split(',');
                        Slot slot = new Slot(new Location(slotAttr[0]), slotAttr[1]);
                        client.joinMeetingSlot(commandAttr[1], slot);
                    }
                    break;

                case "close":
                    Console.WriteLine("Close Meeting");
                    client.closeMeeting(commandAttr[1]);
                    break;

                case "wait":
                    int time = Int32.Parse(commandAttr[1]);
                    Console.WriteLine("Wait");
                    break;
            }
        }

        static void Main(string[] args) {
            Console.WriteLine("ClientScript");
            ClientScript clientScript = new ClientScript();
            clientScript.readClientScript();
            Console.ReadLine();
        }
    }
}