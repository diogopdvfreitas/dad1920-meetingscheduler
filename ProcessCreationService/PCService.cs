using System;
using System.Collections.Generic;
using System.Diagnostics;
using CreationServiceLibrary;

namespace ProcessCreationService {
    public class PCService :MarshalByRefObject, IPCService {
        
        public void createServer(String server_port, String server_id, String server_url, String max_faults, String min_delay, String max_delay) {
            int serverPort = Int32.Parse(server_port);
            int maxFaults = Int32.Parse(max_faults);
            int minDelay = Int32.Parse(min_delay);
            int maxDelay = Int32.Parse(max_delay);

            Console.WriteLine("Launching new server process with id " + server_id);
            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Server\\bin\\Debug\\Server";
            process.StartInfo.Arguments = serverPort + " " + server_id + " " + server_url + " " + maxFaults + " " + minDelay + " " + maxDelay;
            process.Start();

        }

        public void createClient(String clientUsername, String clientUrl, String serverUrl, String scriptFile) {
            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Client\\bin\\Debug\\Client";
            process.StartInfo.Arguments = clientUsername + " " + clientUrl + " " + serverUrl + " " + scriptFile;
            process.Start();

        }
    }
}
