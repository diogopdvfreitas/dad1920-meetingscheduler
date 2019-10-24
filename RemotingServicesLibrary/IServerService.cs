using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectsLibrary;

namespace RemotingServicesLibrary {
    public interface IServerService {

        List<Meeting> listMeetings();
        void createMeeting(String topic, int min_attnd, int nSlots);
        void joinMeeting(String topic);
        void closeMeeting(String topic);
    }
}
