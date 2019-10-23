using System;
using System.Collections;
using System.IO;


namespace Client {
    public class ScriptClient {

        private String scriptName;

        public ScriptClient() { 
        }

        public void readScript() {
            StreamReader script;
            try {
                script = File.OpenText(scriptName);

            }catch (FileNotFoundException) {
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
            String[] commandLine = command.Split(' ');

            ArrayList slots;
            ArrayList invits;
            int nSlots;
            int nInvits;
            int limit1;
            int limit2;

            switch (command[0].ToString()) {

                case "list":
                    Console.WriteLine("Listar");
                    break;

                case "create":
                    nSlots = Int32.Parse(command[3].ToString());
                    nInvits = Int32.Parse(command[4].ToString());

                    slots = new ArrayList();
                    invits = new ArrayList();

                    limit1 = 5 + nSlots;
                    for (int i = 5; i < limit1; i++) {
                        slots.Add(command[i].ToString());
                    }

                    limit2 = limit1 + nInvits;
                    for (int i = limit1; i < limit2; i++) {
                        invits.Add(command[i].ToString());
                    }
                    
                    //falta acrescentar o coordenador 
                    Meeting meeting = new Meeting(command[1].ToString(), Int32.Parse(command[2].ToString()), nSlots, nInvits, slots, invits);
                    break;

                case "join":
                    nSlots = Int32.Parse(command[2].ToString());
                    limit1 = 3 + nSlots;
                    Console.WriteLine("Join Meeting");
                    break;

                case "close":
                    Console.WriteLine("Close Meeting");
                    break;

                case "wait":
                    int time = Int32.Parse(command[1].ToString());
                    Console.WriteLine("Wait");
                    break;
            }
        }

    }
}
