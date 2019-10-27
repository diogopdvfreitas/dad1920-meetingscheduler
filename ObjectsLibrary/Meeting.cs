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
        int _nJoined; //added the number of clients interested in the meeting in order to facilitate the check of the change of status
        Slot _pickedSlot;

        public Meeting(String coord, String topic, int minAtt, List<Slot> slots) {
            _coord = coord;
            _topic = topic;
            _minAtt = minAtt;
            _nSlots = slots.Count;
            _slots = slots;
            _status = Status.OPEN;
            _nJoined = 0;
            _pickedSlot = null;
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
            _nJoined = 0;
            _pickedSlot = null;
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

        public int NJoined {
            get { return _nJoined; } 
        }

        public List<Slot> getSlotsWithEnoughAttendency() {
            List<Slot> slots = new List<Slot>();
            foreach(Slot slot in _slots) {
                if(slot.NJoined >= _minAtt) {
                    slots.Add(slot);
                }
            }
            return slots;
        }

        public bool checkStatusChange(Meeting meeting) {
            return _nJoined != meeting.NJoined || _status != meeting.MStatus;
        }

        public void joinSlot(Slot chosenSlot, String username) {
            foreach(Slot slot in _slots){
                if(slot.Location.Name.Equals(chosenSlot.Location.Name) && slot.Date.Equals(chosenSlot.Date)) { 
                    slot.joinSlot(username);
                    _nJoined++;
                }
            }
        }

        public void close() {
            if(_nJoined >= _minAtt) {
                List<Slot> slotsOK = getSlotsWithEnoughAttendency();
                bool bookingStatus = false;
                while (!bookingStatus) {
                    Random rand = new Random();
                    _pickedSlot = slotsOK[rand.Next(slotsOK.Count)];
                    bookingStatus = _pickedSlot.bookMeeting(this);
                    if (!bookingStatus)
                        slotsOK.Remove(_pickedSlot);
                    if (slotsOK.Count == 0)
                        break;
                }
            }
            _status = Status.CLOSED;
        }

        public String slotsToString(List<Slot> slotsList) {
            String s = "";
            foreach (Slot slot in slotsList)
                s += "\n\t\t" + slot.ToString();
            return s;
        }

        public String inviteesToString(List<String> inviteesList) {
            String s = "";
            foreach (String invitee in inviteesList) {
                s += "\n\t\t" + invitee;
            }
            return s;
        }

        public override String ToString() {
            String s = "Meeting:\n\tCoordinator: " + _coord + "\n\tTopic: " + _topic + "\n\tMin. Attendes: " + _minAtt + "\n\tNumber of Slots: " + _nSlots + "\n\tSlots: " + slotsToString(_slots);

            if(_nInvitees != 0)
                s += "\n\tNumber of Invitees: " + _nInvitees + "\n\tInvitees: " + inviteesToString(_invitees);
            else
                s += "\n\tNumber of Invitees: " + _nInvitees;

            if(_status == Status.OPEN)
                s += "\n\tStatus: Open";
            else {
                s += "\n\tStatus: Closed";
                if (_pickedSlot != null)
                    s += "\n\t\tMeeting at " + _pickedSlot + " in " + _pickedSlot.PickedRoom.Name;
                else
                    s += "\n\t\tMeeting couldn't be scheduled";            }
            
            return s + "\n";
        }
    }
}
