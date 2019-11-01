using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationServiceLibrary
{
    public interface IPCService{
        void createClient(String clienUsername, String clientUrl, String serverUrl, String scriptFile);
    }
}
