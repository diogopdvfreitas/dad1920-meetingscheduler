using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace ProcessCreationService {
    public class PCS {
        private TcpChannel _channel;
        private PCService _pcService;
        public PCS() {
            _channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(_channel, false);
            _pcService = new PCService();
            RemotingServices.Marshal(_pcService, "PCSERVICE", typeof(PCService));
        }
        static void Main(string[] args) {
            Console.WriteLine("PCS");
            PCS pcs = new PCS();
            Console.ReadLine(); 
        }
    }
}
