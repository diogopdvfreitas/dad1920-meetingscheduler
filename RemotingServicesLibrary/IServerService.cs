using System;
using System.Collections.Generic;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IServerService {
        //void executeCommand(String senderServer, String command, List<Object> args);
        void clientConnect(String username, String clientUrl);
        IDictionary<String, String> getRegisteredClients();
        Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots);
        Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees);
        Meeting getMeeting(String topic);
        bool checkMeetingStatusChange(Meeting meeting);
        Meeting joinMeetingSlot(String topic, String slot, String username);
        void closeMeeting(String topic);
        void addRoom(String location, int capacity, String name);
    }
}
