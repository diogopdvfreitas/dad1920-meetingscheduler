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
        Status _status;
        int _nInterested; //added the number of clients interested in the meeting in order to facilitate the check of the change of status

        public Meeting(String coord, String topic, int minAtt, List<Slot> slots) {
            _coord = coord;
            _topic = topic;
            _minAtt = minAtt;
            _nSlots = slots.Count;
            _slots = slots;
            _status = Status.OPEN;
            _nInterested = 0;
        }

        public Meeting(String coord, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            _coord = coord;
            _topic = topic;
            _minAtt = minAtt;
            _nSlots = slots.Count;
            _slots = slots;
            _nInvitees = invitees.Count;
            _invitees = invitees;
            _status = Status.OPEN;
            _nInterested = 0;
        }

        public String Coord {
            get { return _coord; }
        }
        public String Topic {
            get { return _topic; }
        }

        public Status MStatus {
            get { return _status; }
            set { _status = value; }
        }

        public List<Slot> Slots {
            get { return _slots; }
        }

        public int NInterested {
            get { return _nInterested; } 
        }

        public bool checkStatusChange(Meeting meeting) {
            return _nInterested != meeting.NInterested || _status != meeting.MStatus;
        }

        public List<Slot> getSlots{
            get { return _slots; }
        }

        public void joinSlot(Slot chosenSlot, String username) {
            foreach(Slot slot in _slots){
                if(slot.Location.Name.Equals(chosenSlot.Location.Name) && slot.Date.Equals(chosenSlot.Date)) { 
                    slot.NewInterested(username);
                    _nInterested++;
                }
            }
        }

        public String slotsToString(List<Slot> slotsList) {
            String s = "";
            foreach (Slot st in slotsList)
                s += "\n\t\t" + st.ToString();
            return s;
        }

        public String inviteesToString(List<String> inviteesList) {
            String s = "";
            foreach (String i in inviteesList) {
                s += "\n\t\t" + i;
            }
            return s;
        }

        public override String ToString() {
            if(_nInvitees != 0)
                return "Meeting:\n\tCoordinator: " + _coord + "\n\tTopic: " + _topic + "\n\tMin. Attendes: " + _minAtt + "\n\tNumber of Slots: " + _nSlots + "\n\tNumber of Invitees: " + _nInvitees + "\n\tSlots: " + slotsToString(_slots) + "\n\tInvitees: " + inviteesToString(_invitees) + "\n";
            else
                return "Meeting:\n\tCoordinator: " + _coord + "\n\tTopic: " + _topic + "\n\tMin. Attendes: " + _minAtt + "\n\tNumber of Slots: " + _nSlots + "\n\tNumber of Invitees: " + _nInvitees + "\n\tSlots: " + slotsToString(_slots) + "\n";
        }

    }
}
