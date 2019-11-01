using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationServiceLibrary
{
    public interface IPCService{

        void createServer(String server_port, String server_id, String server_url, String max_faults, String min_delay, String max_delay);
        void createClient(String clienUsername, String clientUrl, String serverUrl, String scriptFile);
    }
}
