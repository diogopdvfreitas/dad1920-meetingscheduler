using System;
using ObjectsLibrary;

namespace Server {
    public class DelayedMessage {
        String _delayedServer;
        private int _currentTimeStamp;
        private int _futureTimeStamp;
        private Meeting _meeting;

        public DelayedMessage(String delayedServer, int currentTimeStamp, int futureTimeStamp, Meeting meeting) {
            _delayedServer = delayedServer;
            _currentTimeStamp = currentTimeStamp;
            _futureTimeStamp = futureTimeStamp;
            _meeting = meeting;

        }

        public int CurrentTimeStamp {
            get { return _currentTimeStamp; }
        }

        public int FutureTimeStamp {
            get { return _futureTimeStamp; }
        }

        public Meeting Meeting {
            get { return _meeting; }
        }

    }
}
