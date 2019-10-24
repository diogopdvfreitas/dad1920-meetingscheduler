using System;
using System.Collections.Generic;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IServerService {
        bool checkMeetingStatus(Meeting meeting);

        Meeting getMeeting(String meetingTopic);

        void createMeeting(Meeting meeting);

        //void joinMeeting(String topic);
        //void closeMeeting(String topic);
    }
}
