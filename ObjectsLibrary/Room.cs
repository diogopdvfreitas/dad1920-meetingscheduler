﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool bookMeeting(String date, Meeting meeting) {
            if (!_meetingSchedule.ContainsKey(date)) {
                _meetingSchedule.Add(date, meeting.Topic);
                return true;
            }
            return false;
        }
    }
}