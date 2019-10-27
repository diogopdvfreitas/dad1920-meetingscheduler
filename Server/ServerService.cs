using System;
using RemotingServicesLibrary;
using ObjectsLibrary;
using System.Collections.Generic;

namespace Server {
    public class ServerService : MarshalByRefObject, IServerService {
        private IDictionary<String, Meeting> _meetings;
        private List<String> _clients;

        public ServerService() {
            _meetings = new Dictionary<String, Meeting>();
            _clients = new List<String>();
        }

        public void connect(String username, String clientUrl) {
            String client = username + "-" + clientUrl;
            _clients.Add(client);
        }

        public List<String> getRegisteredClients() {
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
