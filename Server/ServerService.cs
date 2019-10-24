using System;
using RemotingServicesLibrary;
using ObjectsLibrary;
using System.Collections;

namespace Server {
    public class ServerService : MarshalByRefObject, IServerService {
        private Hashtable meetings;

        public ServerService() {
            meetings = new Hashtable();
            
        }

        //checkMeetingStatus: check if the meeting status has changed or not; if has not changed the received meeting is return
        //otherwise the changed meeting is return
        public bool checkMeetingStatus(Meeting meeting) {
            return meeting.checkStatus((Meeting)meetings[meeting.Topic]);
        }

        public Meeting getMeeting(String meetingTopic) {
            return (Meeting) meetings[meetingTopic];
        }
        
        public void createMeeting(Meeting meeting) {
            Console.WriteLine(meeting.print());
            meetings.Add(meeting.Topic, meeting);
        }
    }
}
