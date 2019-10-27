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

        public void connect(String clientUrl) {
            _clients.Add(clientUrl);
        }

        public List<String> getRegisteredClients() {
            return _clients;
        }

        //checkMeetingStatus: check if the meeting status has changed or not
        public bool checkMeetingStatusChange(Meeting meeting) {
            return meeting.checkStatusChange(_meetings[meeting.Topic]);
        }

        public Meeting getMeeting(String meetingTopic) {
            return _meetings[meetingTopic];
        }
        
        public void createMeeting(Meeting meeting) {
            Console.WriteLine(meeting.ToString());
            _meetings.Add(meeting.Topic, meeting);
        }

        public void joinMeeting(String meetingTopic, Slot slot, String username) {
            _meetings[meetingTopic].joinSlot(slot, username);
        }


    }
}
