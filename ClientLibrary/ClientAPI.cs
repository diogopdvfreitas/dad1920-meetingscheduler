using System;
using System.Collections.Generic;
using ObjectsLibrary;

namespace ClientLibrary{
    public interface ClientAPI{
        void listMeetings();
        void createMeeting(String topic, int minAtt, List<Slot> slots);
        void createMeeting(String topic, int minAtt, List<Slot> slots, List<String> invitees);
        void joinMeetingSlot(String topic, String chosenSlot);
        void closeMeeting(String topic);
        void printStatus();

    }
}
