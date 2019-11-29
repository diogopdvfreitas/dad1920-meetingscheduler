using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsLibrary {
    [Serializable]
    public class MeetingMessage {

        private Meeting _meeting;
        private IDictionary<String, int> _vectorClock;
        private String _serverUrl;

        public MeetingMessage(String serverUrl, IDictionary<String, int> vectorClock, Meeting meeting) {
            _serverUrl = serverUrl;
            _vectorClock = vectorClock;
            _meeting = meeting;
        }

        public String Server {
            get { return _serverUrl; }
            set { _serverUrl = value; }
        }

        public IDictionary<String, int> VectorClock {
            get { return _vectorClock; }
            set { _vectorClock = value; }
        }

        public Meeting Meeting {
            get { return _meeting; }
            set { _meeting = value; }
        }
    }
}
