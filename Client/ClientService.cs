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
        public void receiveInvite(Meeting meeting) {
            Console.WriteLine("Recebeu o convite da reuniao " + meeting.Topic);
            _client.receiveInvite(meeting);
        }
    }
}
