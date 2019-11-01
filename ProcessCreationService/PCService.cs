using System;
using System.Collections.Generic;
using System.Diagnostics;
using CreationServiceLibrary;

namespace ProcessCreationService {
    public class PCService :MarshalByRefObject, IPCService {
        
        public void createClient(String clientUsername, String clientUrl, String serverUrl, String scriptFile) {
            Process process = new Process();
            process.StartInfo.FileName = "..\\..\\..\\Client\\bin\\Debug\\Client";
            process.StartInfo.Arguments = clientUsername + " " + clientUrl + " " + serverUrl + " " + scriptFile;
            process.Start();

        }
    }
}
