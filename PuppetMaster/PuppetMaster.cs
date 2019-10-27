using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster {
    class PuppetMaster {

        private TcpChannel _channel;

        public PuppetMaster() {
            _channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(_channel, false);
        }

        public void executeCommand(String command) {
            String[] commandAttr = command.Split(' ');

            switch (commandAttr[0]) {
                case "Server":
                    break;
                case "Client":
                    break;
                case "AddRoom":
                    break;
                case "Status":
                    break;
                case "Crash":
                    break;
                case "Freeze":
                    break;
                case "Unfreeze":
                    break;
                case "Wait":
                    break;
            }
        }
        static void Main(string[] args) {
        }
    }
}
