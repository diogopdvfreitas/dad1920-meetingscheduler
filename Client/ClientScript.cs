﻿using System;
using System.Collections.Generic;
using System.IO;
using ObjectsLibrary;

namespace Client {
    public class ClientScript {

        Client _client;

        private String _scriptName = "scriptclient.txt";      // Default script name

        public ClientScript() {
            _client = new Client();
        }

        public ClientScript(String username, String clientUrl, String serverUrl, String scriptName) {
            _scriptName = scriptName;
            _client = new Client(username, clientUrl, serverUrl);
        }

        public void readClientScript() {
            StreamReader script;
            try {
                script = File.OpenText("../../../" + _scriptName);
            }
            catch (FileNotFoundException) {
                Console.WriteLine("File: " + _scriptName +  " Not Found");
                return;
            }

            String scriptLine = script.ReadLine();
            while (scriptLine != null) {
                Console.WriteLine("[COMMAND] " + scriptLine);
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
                case "list":
                    _client.listMeetings();
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
                        Slot slot = new Slot(slotAttr[0], slotAttr[1]);
                        slots.Add(slot);
                    }

                    nInvitees = Int32.Parse(commandAttr[4].ToString());
                    // Invitees are optional
                    if (nInvitees == 0) {
                        _client.createMeeting(commandAttr[1], Int32.Parse(commandAttr[2]), slots);
                    }else {
                        invitees = new List<String>();

                        // Select the invitees from the command
                        invtsStartIndex = slotsStartIndex + nInvitees;
                        for (int i = slotsStartIndex; i < invtsStartIndex; i++) {
                            invitees.Add(commandAttr[i]);
                        }
                        _client.createMeeting(commandAttr[1], Int32.Parse(commandAttr[2]), slots, invitees);
                    }
                    break;

                case "join":
                    nSlots = Int32.Parse(commandAttr[2]);

                    for (int i = 3; i < 3 + nSlots; i++) {
                        _client.joinMeetingSlot(commandAttr[1], commandAttr[i]);
                    }
                    break;

                case "close":
                    Console.WriteLine("Close Meeting");
                    _client.closeMeeting(commandAttr[1]);
                    break;

                case "wait":
                    int time = Int32.Parse(commandAttr[1]);
                    _client.wait(time);
                    Console.WriteLine("Wait");
                    break;
            }
        }

        static void Main(string[] args) {
            Console.WriteLine("[CLIENT SCRIPT]");
            ClientScript clientScript;
            if (args.Length == 0)
                clientScript = new ClientScript();
            else {
                clientScript = new ClientScript(args[0], args[1], args[2], args[3]);
            }
            clientScript.readClientScript();
            Console.WriteLine("QUIT to exit");
            while (true) {
                Console.Write("[INSERT NEW COMMAND:] ");
                String line = Console.ReadLine();
                if (line == "QUIT")
                    break;
                else
                    clientScript.executeCommand(line);
            }
        }
    }
}