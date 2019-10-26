using System;
using System.Collections.Generic;

namespace ClientLibrary{
    public interface ClientAPI{
        void listMeeting();

        void createMeeting(String topic, int minAtt, int nSlots, int nInvits, List<String> slots, List<String> invits);
    }
}
