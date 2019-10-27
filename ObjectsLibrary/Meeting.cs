using System;
using System.Collections.Generic;

namespace ObjectsLibrary {
    [Serializable]
    public class Meeting {
        public enum Status {
            OPEN = 1,
            CLOSED = 0
        }

        String _coord;
        String _topic;
        int _minAtt;
        int _nSlots;
        int _nInvitees;
        List<Slot> _slots;
        List<String> _invitees;
        Status _Status;

        public Meeting(String coord, String topic, int minAtt, List<Slot> slots) {
            _coord = coord;
            _topic = topic;
            _minAtt = minAtt;
            _nSlots = slots.Count;
            _slots = slots;
            _Status = Status.OPEN;
        }

        public Meeting(String coord, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            _coord = coord;
            _topic = topic;
            _minAtt = minAtt;
            _nSlots = slots.Count;
            _slots = slots;
            _nInvitees = invitees.Count;
            _invitees = invitees;
            _Status = Status.OPEN;
        }

        public String Coord {
            get { return _coord; }
        }

        public String Topic {
            get { return _topic; }
        }

        public Status MStatus {
            get { return _Status; }
            set { _Status = value; }
        }

        public bool checkStatusChange(Meeting meeting) {
            return _Status != meeting.MStatus;
        }

        public List<Slot> getSlots{
            get { return _slots; }
        }

        public void joinSlot(Slot chosenSlot, String username) {
            foreach(Slot slot in _slots){
                if(slot.Location.Equals(chosenSlot.Location) && slot.Date.Equals(chosenSlot.Date)) {
                    slot.NewInterested(username);
                }
            }
        }

        public override String ToString() {
            return "Meeting:\n\tCoordinator: " + _coord + "\n\tTopic: " + _topic + "\n\tMin. Attendes: " + _minAtt + "\n\tNumber of Slots: " + _nSlots + "\n\tNumber of Invitees: " + _nInvitees + "\n\tSlots: " + _slots.ToString() + "\n\tInvitees: " + _invitees.ToString() + "\n";
        }
    }
}
