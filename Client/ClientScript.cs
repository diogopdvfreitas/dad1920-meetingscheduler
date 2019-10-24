using System;
using System.IO;
using System.Collections;
using ObjectsLibrary;

namespace Client {
    public class ClientScript {

        private String _scriptName;

        public ClientScript(String scriptName) {
            _scriptName = scriptName;
        }

        public void readScript() {
            StreamReader script;
            try {
                script = File.OpenText(_scriptName);

            } catch (FileNotFoundException) {
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

            ArrayList slots;
            ArrayList invits;
            int nSlots;
            int nInvits;
            int limit1;
            int limit2;

            switch (commandAttr[0].ToString()) {
                //TODO command creation on ClientService.cs
                case "list":
                    Console.WriteLine("List");
                    break;

                case "create":
                    nSlots = Int32.Parse(commandAttr[3].ToString());
                    nInvits = Int32.Parse(commandAttr[4].ToString());

                    slots = new ArrayList();
                    invits = new ArrayList();

                    limit1 = 5 + nSlots;
                    for (int i = 5; i < limit1; i++) {
                        slots.Add(commandAttr[i].ToString());
                    }

                    limit2 = limit1 + nInvits;
                    for (int i = limit1; i < limit2; i++) {
                        invits.Add(commandAttr[i].ToString());
                    }

                    //falta acrescentar o coordenador que pode ser o url do cliente
                    Meeting meeting = new Meeting(commandAttr[1].ToString(), Int32.Parse(commandAttr[2].ToString()), nSlots, nInvits, slots, invits);
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

    }
}
