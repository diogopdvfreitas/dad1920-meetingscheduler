using System;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IClientService {
        void receiveInvite(Meeting meeting);
        String status();
    }
}
