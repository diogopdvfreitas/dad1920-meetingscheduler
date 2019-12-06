using System;
using System.Collections.Generic;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IServerService {
        IDictionary<String, int> clientConnect(String username, String clientUrl);
        void receiveNewClient(String username, String clientUrl);
        IDictionary<String, String> getRegisteredClients();
		IDictionary<String, IServerService> getRegisteredServers();
        IDictionary<String, int> getVectorClock();
        MeetingMessage createMeeting(String username, String topic, int minAtt, List<Slot> slots);
        MeetingMessage createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees);
        MeetingMessage joinMeetingSlot(String topic, String slot, String username);
        MeetingMessage closeMeeting(String topic, String username);
        Meeting getMeeting(String topic);
        bool checkMeetingStatusChange(Meeting meeting);
        void receiveChanges(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings);
        void addRoom(String location, int capacity, String name);
        void addLocation(String location_name, Location location);
        IDictionary<String, int> updateServer();
        void getUpdatedMeetingsFromUpdatedServer(String requestingServerURL, IDictionary<String, int> vectorClock);
        String status();
        void freeze();
        void unfreeze();
        KeyValuePair<int, String> grantCloseTicket(String serverUrl);
        void newGrantedTicket(String leader, KeyValuePair<int, String> newGrantedTicketByLeader);
        void selectNewLeader();
    }
}
