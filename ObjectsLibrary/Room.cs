using System;
using System.Collections.Generic;

namespace ObjectsLibrary {
    [Serializable]
    public class Room {
        private String _name;
        private int _capacity;
        private IDictionary<String, String> _meetingSchedule; // Key - Meeting Date; Value - Meeting Topic

        public Room(String name, int capacity) {
            _name = name;
            _capacity = capacity;
            _meetingSchedule = new Dictionary<String, String>();
        }

        public String Name {
            get { return _name; }
        }

        public int Capacity {
            get { return _capacity; }
        }

        public bool checkRoomFree(String date) {
            return !_meetingSchedule.ContainsKey(date);
        }

        public bool bookMeeting(String date, Meeting meeting) {
            if (!_meetingSchedule.ContainsKey(date)) {
                _meetingSchedule.Add(date, meeting.Topic);
                return true;
            }
            return false;
        }

        public override String ToString() {
            String s = "\t\tRoom " + _name + ":\n\t\t\tCapacity - " + _capacity + "\n\t\t\tSchedule:\n";

            foreach(KeyValuePair<String, String> meetingScheduled in _meetingSchedule) {
                s += "\t\t\t\t" + meetingScheduled.Value + " on " + meetingScheduled.Key + "\n";
            }

            return s;
        }
    }
}
