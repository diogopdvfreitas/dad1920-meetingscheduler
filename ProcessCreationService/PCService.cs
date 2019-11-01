using System;
using System.Diagnostics;
using CreationServiceLibrary;

namespace ProcessCreationService {
    public class PCService :MarshalByRefObject, IPCService {
        
        public void createClient(String clienUsername, String clientUrl, String serverUrl, String scriptFile) {
            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Client\\bin\\Debug\\Client";
            

        }
    }
}
