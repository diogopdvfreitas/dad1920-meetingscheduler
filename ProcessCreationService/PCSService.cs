﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using CreationServiceLibrary;

namespace ProcessCreationService {
    public class PCSService :MarshalByRefObject, IPCSService {

        private IDictionary<String, Process> _processes;        // <ID, Process>
        private IDictionary<String, String> _serverUrls;        // <Server ID, Server URL>
        private IDictionary<String, String> _clientUrls;        // <Client Username, Client URL>

        public PCSService() {
            _processes = new Dictionary<String, Process>();
            _serverUrls = new Dictionary<String, String>();
            _clientUrls = new Dictionary<String, String>();
        }
                
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
            _processes.Add(server_id, process);
            _serverUrls.Add(server_id, server_url);
        }

        public void createClient(String clientUsername, String clientUrl, String serverUrl, String scriptFile) {
            Console.WriteLine("Client Username: " +clientUsername);
            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Client\\bin\\Debug\\Client";
            process.StartInfo.Arguments = clientUsername + " " + clientUrl + " " + serverUrl + " " + scriptFile;
            process.Start();
            _processes.Add(clientUsername, process);
            _serverUrls.Add(clientUsername, clientUrl);
        }

        public IDictionary<String, Process> Processes {
            get { return _processes; }
        }

        public IDictionary<String, String> ServerURLs {
            get { return _serverUrls; }
        }

        public IDictionary<String, String> ClientURLs {
            get { return _clientUrls; }
        }
    }
}