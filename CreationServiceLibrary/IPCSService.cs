using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationServiceLibrary
{
    public interface IPCSService{

        void createServer(String server_port, String server_id, String server_url, String max_faults, String min_delay, String max_delay, String server_obj);
        void createClient(String clienUsername, String clientUrl, String serverUrl, String scriptFile);
        void shutDown();
        IDictionary<String, Process> Processes { get; }
        IDictionary<String, String> ServerURLs { get; }
        IDictionary<String, String> ClientURLs { get; }
    }
}
