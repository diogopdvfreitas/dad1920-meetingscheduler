using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsLibrary {
    public class Room {
        private String _name;
        private int _capacity;
        private IDictionary<String, String> _meetingSchedule; // Key - Meeting Topic; Value - Meeting Date

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

        public void bookMeeting(Meeting meeting, String date) {
            _meetingSchedule.Add(meeting.Topic, date);
        }
    }
}
