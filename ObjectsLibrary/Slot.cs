using System;
using System.Collections.Generic;

namespace ObjectsLibrary {

    [Serializable]
    public class Slot {
        private Location _location;
        private String _date;
        private List<String> _interested;

        public Slot(Location location, String date) {
            _location = location;
            _date = date;
            _interested = new List<String>();
        }

        public Location Location {
            get { return _location; }
            set { _location = value; }
        }

        public String Date {
            get { return _date; }
            set { _date = value; }
        }

        public int NrOfInterested {
            get { return _interested.Count; }
        }

        public List<String> Interested {
            get { return _interested; }
        }

        public void NewInterested(String clientName) {
            _interested.Add(clientName);
        }

        public override String ToString() {
            return _location.Name + ", " + _date + ", Number of Interested Clients: " + NrOfInterested;
        }
    }
}
