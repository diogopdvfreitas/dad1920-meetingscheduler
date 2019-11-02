using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  ObjectsLibrary{
    public class ServerSnapshot {
        private String _originServerId;
        private DateTime _timestamp;

        private IDictionary<String, Meeting> _meetings;

        public ServerSnapshot(String originServerId, IDictionary<String, Meeting> meetings) {
            _originServerId = originServerId;
            _timestamp = DateTime.Now;
            _meetings = meetings;
        }

        public String OriginServerId {
            get { return _originServerId; }
        }

        public DateTime Timestamp {
            get { return _timestamp; }
        }

        public IDictionary<String, Meeting> Meetings {
            get { return _meetings; }
        }
    }
}
