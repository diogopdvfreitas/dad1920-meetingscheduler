using System;
using System.Collections.Generic;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IServerService {
        void connect(String username, String clientUrl);
        Dictionary<String, String> getRegisteredClients();
        bool checkMeetingStatusChange(Meeting meeting);
        Meeting getMeeting(String topic);
        void createMeeting(Meeting meeting);
        void joinMeetingSlot(String topic, Slot slot, String username);
        void closeMeeting(String topic);
    }
}
