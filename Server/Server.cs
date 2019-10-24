using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace Server {
    public class Server {
        static void Main(string[] args) {
            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, false);

            ServerService serverService = new ServerService();
            RemotingServices.Marshal(serverService, "SERVER", typeof(ServerService));

            Console.ReadLine();

        }
    }
}
