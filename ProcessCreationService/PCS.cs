using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace ProcessCreationService {
    public class PCS {
        private TcpChannel _channel;
        private PCSService _pcsService;

        public PCS() {
            _channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(_channel, false);
            _pcsService = new PCSService();
            RemotingServices.Marshal(_pcsService, "PCSERVICE", typeof(PCSService));
        }

        static void Main(string[] args) {
            Console.WriteLine("PCS");
            PCS pcs = new PCS();
            Console.ReadLine(); 
        }
    }
}
