using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace Server {
    public class Server {

        private String _id = "SERVER";
        private String _url;
        private int _port = 8086;  
        private int _min_delay = 0;
        private int _max_delay = 0;
        
        public Server() {
            setServer();
        }


        public Server(String id, String url, int min_delay, int max_delay) {
            _id = id;
            _url = url;
            _port = 10000;
            _min_delay = min_delay;
            _max_delay = max_delay;
            setServer();
        }

        private void setServer() {
            TcpChannel channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(channel, false);

            ServerService serverService = new ServerService(_min_delay, _max_delay);
            RemotingServices.Marshal(serverService, _id, typeof(ServerService));
            
            Console.WriteLine("Server " + _id + " created at port " + _port + " with delay from " + _min_delay + " to " + _max_delay);
        }

        static void Main(string[] args) {
            if(args.Length == 0) {
                Server server = new Server();
            }
            else{
                Server server = new Server(args[0], args[1], Int32.Parse(args[2]), Int32.Parse(args[3]));
            }
            Console.ReadLine();

        }
    }
}
