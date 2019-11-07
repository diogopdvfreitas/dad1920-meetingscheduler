using System;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IClientService {
        void receiveInvitee(Meeting meeting);
        void listMeetings();
        String getUsername();

        String status();
    }
}
