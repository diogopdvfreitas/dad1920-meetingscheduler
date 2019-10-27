using System;
using System.Collections.Generic;

namespace ObjectsLibrary {

    [Serializable]
    public class Slot {
        private Location _location;
        private String _date;
        private List<String> _joined;
        private Room _pickedRoom;

        public Slot(Location location, String date) {
            _location = location;
            _date = date;
            _joined = new List<String>();
            _pickedRoom = null;
        }

        public Location Location {
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
        }

        public Room PickedRoom {
            get { return _pickedRoom; }
        }

        public void joinSlot(String clientName) {
            _joined.Add(clientName);
        }

        public bool bookMeeting(Meeting meeting) {
            List<Room> roomsOK = _location.getRoomsWithEnoughCapacity(this.NJoined);
            bool bookingStatus = false;
            while(!bookingStatus) {
                Random rand = new Random();
                _pickedRoom = roomsOK[rand.Next(roomsOK.Count)];
                bookingStatus = _pickedRoom.bookMeeting(_date, meeting);
                if(!bookingStatus)
                    roomsOK.Remove(_pickedRoom);
                if(roomsOK.Count == 0)
                    break;
            }
            return bookingStatus;
        }

        public override String ToString() {
            return _location.Name + "," + _date + ": Number of Joins - " + NJoined;
        }
    }
}
