using RemotingServicesLibrary;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace Server {
    public class Server {

        //default values
        private String _id = "SERVER";
        private String _url = "tcp://localhost:8086/SERVER";
        private int _port = 8086;
        private int _max_faults = 0;
        private int _min_delay = 0;
        private int _max_delay = 0;
        TcpChannel _channel;
        
        public Server() {
            setServer();
        }


        public Server(int port, String id, String url, int max_faults, int min_delay, int max_delay) {
            _port = port;
            _id = id;
            _url = url;
            _max_faults = max_faults;
            _min_delay = min_delay;
            _max_delay = max_delay;
            setServer();
        }

        public String Url { 
            get { return _url; }
        }

        private void setServer() {
            _channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(_channel, false);

            ServerService serverService = new ServerService(this, _min_delay, _max_delay);
            RemotingServices.Marshal(serverService, _id, typeof(ServerService));
            
            Console.WriteLine("Server " + _id + " created at port " + _port + " with delay from " + _min_delay + " to " + _max_delay);
        }

        static void Main(string[] args) {
            if(args.Length == 0) {
                Server server = new Server();
            }
            else{
                Server server = new Server(Int32.Parse(args[0]), args[1], args[2], Int32.Parse(args[3]), Int32.Parse(args[4]), Int32.Parse(args[5]));
            }
            Console.ReadLine();

        }
    }
}
