using System;
using System.Collections.Generic;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IServerService {
        void connect(String clientUrl);

        List<String> getRegisteredClients();

        bool checkMeetingStatusChange(Meeting meeting);

        Meeting getMeeting(String meetingTopic);

        void createMeeting(Meeting meeting);


        //void joinMeeting(String topic);
        //void closeMeeting(String topic);
    }
}
