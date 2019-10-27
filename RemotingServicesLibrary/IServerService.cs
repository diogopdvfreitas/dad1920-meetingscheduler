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


        void joinMeeting(String meetingTopic, Slot slot, String username);
        //void closeMeeting(String topic);
    }
}
