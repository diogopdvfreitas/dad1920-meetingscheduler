using System;
using System.Collections.Generic;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IServerService {
        void clientConnect(String username, String clientUrl);
        void receiveNewClient(String username, String clientUrl);
        IDictionary<String, String> getRegisteredClients();
        Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots);
        Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees);
        Meeting joinMeetingSlot(String topic, String slot, String username);
        Meeting closeMeeting(String topic, String username);
        Meeting getMeeting(String topic);
        bool checkMeetingStatusChange(Meeting meeting);
        void receiveChanges(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings, int serverCloseTicket);
        void addRoom(String location, int capacity, String name);
        void addLocation(String location_name, Location location);
        String status();
        void freeze();
        void unfreeze();
        KeyValuePair<int, String> grantCloseTicket(String serverUrl);
        void newGrantedTicket(String leader, KeyValuePair<int, String> newGrantedTicketByLeader);
        void selectNewLeader();
    }
}
