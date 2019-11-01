using System;
using RemotingServicesLibrary;
using ObjectsLibrary;
using System.Collections.Generic;

namespace Server {
    public class ServerService : MarshalByRefObject, IServerService {

        private Server _server;
        private IDictionary<String, Meeting> _meetings;
        private Dictionary<String, String> _clients;
        private int _min_delay;
        private int _max_delay;

        public ServerService() {
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();
        }

        public ServerService(Server server, int min_delay, int max_delay) {
            _meetings = new Dictionary<String, Meeting>();
            _clients = new Dictionary<String, String>();
            _server = server;
            _min_delay = min_delay;
            _max_delay = max_delay;
        }

        public void connect(String username, String clientUrl) {
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
        }

        public void joinMeetingSlot(String topic, Slot slot, String username) {
            _meetings[topic].joinSlot(slot, username);
        }

        public void closeMeeting(String topic) {
           _meetings[topic].close();            
        }
    }
}
