using System;
using RemotingServicesLibrary;
using ObjectsLibrary;
using System.Collections.Generic;
using System.Configuration;

namespace Server {
    public class ServerService : MarshalByRefObject, IServerService {

        private Server _server;
        private IDictionary<String, Meeting> _meetings;
        private Dictionary<String, String> _clients;
        private Dictionary<String, IServerService> _otherServers;

        private int _min_delay;
        private int _max_delay;

        public ServerService() {
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();
            _otherServers = new Dictionary<String, IServerService>();

            serversConfig();
        }

        public ServerService(Server server, int min_delay, int max_delay) {
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();
            _otherServers = new Dictionary<String, IServerService>();

            _server = server;
            _min_delay = min_delay;
            _max_delay = max_delay;

            serversConfig();
        }

        //serversConfig: gets a proxy for each known server
        public void serversConfig() {
            foreach (String serverUrl in ConfigurationManager.AppSettings) {
                if (serverUrl != _server.Url) {
                    String[] urlAttributes = serverUrl.Split(new Char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    IServerService serverServ = (IServerService)Activator.GetObject(typeof(IServerService), serverUrl);
                    _otherServers.Add(serverUrl, serverServ); //neste momento a key é o url, mas acho que nao vai ser, provavelmente vai ser o id
                }
            }
        }

        public void informServers(String command, List<Object> args) {
            foreach (IServerService serverServ in _otherServers.Values) {
                serverServ.executeCommand(command, args);
            }
        }

        public void executeCommand(String command, List<Object> args) {

            switch (command) {
                case "create":
                    createMeeting((Meeting)args[0]);
                    break;

                case "join":
                    joinMeetingSlot((String)args[0], (Slot) args[1], (String)args[2]);
                    break;

                case "close":
                    closeMeeting((String)args[0]);
                    break;
            }
        }

        public void clientConnect(String username, String clientUrl) {
            _clients.Add(username, clientUrl);
        }

        public Dictionary<String, String> getRegisteredClients() {
            return _clients;
        }

        public Meeting getMeeting(String topic) {
            return _meetings[topic];
        }

        //checkMeetingStatus: check if the meeting status has changed or not
        public bool checkMeetingStatusChange(Meeting meeting) {
            return meeting.checkStatusChange(_meetings[meeting.Topic]);
        }

        public void createMeeting(Meeting meeting) {
            Console.WriteLine(meeting.ToString());
            _meetings.Add(meeting.Topic, meeting);
            List<Object> args = new List<Object>(){ meeting };
            //informServers("create", args);
        }

        public void joinMeetingSlot(String topic, Slot slot, String username) {
            _meetings[topic].joinSlot(slot, username);
            List<Object> args = new List<Object>() { topic, slot, username };
            //informServers("join", args);
        }

        public void closeMeeting(String topic) {
           _meetings[topic].close();
            List<Object> args = new List<Object>() { topic };
           // informServers("close", args);
        }
    }
}
