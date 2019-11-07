using System;
using RemotingServicesLibrary;
using ObjectsLibrary;

namespace Client {
    public class ClientService : MarshalByRefObject, IClientService {
        private Client _client;

        public ClientService(Client client) {
            _client = client; 
        }
        //receiveInvite: receive meeting invitee
        public void receiveInvitee(Meeting meeting) {
            Console.WriteLine("Received an invitation to the meeting " + meeting.Topic);
            _client.receiveInvitee(meeting);
        }

        public void listMeetings() {
            _client.listMeetings();
        }

        public String getUsername() {
           return _client.getUsername();
        }

        public String status() {
            return _client.status();
        }
    }
}
