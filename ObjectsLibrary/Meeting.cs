using System;
using System.Collections.Generic;

namespace ObjectsLibrary {
    [Serializable]
    public class Meeting {
        String _coord;
        String _topic;
        int _minAtt;
        int _nSlots;
        int _nInvit;
        List<String> _slots;
        List<String> _invits;
        bool _closed; //false: the meeting is still open; true: the meeting is closed;

        public Meeting(String coord, String topic, int minAtt, int nSlots, int nInvit, List<String> slots, List<String> invits) {
            _coord = coord;
            _topic = topic;
            _minAtt = minAtt;
            _nSlots = nSlots;
            _nInvit = nInvit;
            _slots = slots;
            _invits = invits;
            _closed = false;
        }

        public String Coord {
            get { return _coord; }
        }

        public String Topic {
            get { return _topic; }
        }

        public bool Closed {
            get { return _closed; }
            set { _closed = value; }
        }

        public bool checkStatus(Meeting m) {
            return _closed != m.Closed;
        }
        
        public String printList(List<String> list) {
            String stringList = "";
            foreach (String s in list) {
                stringList += s + " ";
            }
            return stringList;
        }

        public String print() {
            return "Meeting: Coordinator: " + _coord + " Topic: " + _topic + " Min Attendes: " + _minAtt + " N Slots: " + _nSlots + " N Invitees: " + _nInvit + " Slots: " + printList(_slots) + "Invitees " + printList(_invits);
        }


    }
}
