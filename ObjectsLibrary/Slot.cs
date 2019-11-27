using System;
using System.Collections.Generic;

namespace ObjectsLibrary {
    [Serializable]
    public class Slot {
        private String _location;
        private String _date;
        private List<String> _joined;
        private Room _pickedRoom;

        public Slot(String location, String date) {
            _location = location;
            _date = date;
            _joined = new List<String>();
            _pickedRoom = null;
        }

        public String Location {
            get { return _location; }
            set { _location = value; }
        }

        public String Date {
            get { return _date; }
            set { _date = value; }
        }

        public int NJoined {
            get { return _joined.Count; }
        }

        public List<String> Joined {
            get { return _joined; }
            set { _joined = value; }
        }

        public Room PickedRoom {
            get { return _pickedRoom; }
            set { _pickedRoom = value; }
        }

        public void joinSlot(String clientName) {
            _joined.Add(clientName);
        }

        public override String ToString() {
            return _location + "," + _date + ": Number of Joins - " + NJoined;
        }
    }
}
