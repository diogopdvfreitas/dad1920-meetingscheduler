using System;
using System.Collections.Generic;

namespace ObjectsLibrary {
    [Serializable]
    public class Location {
        private String _name;
        private List<Room> _rooms;

        public Location(String name) {
            _name = name;
            _rooms = new List<Room>();
        }

        public String Name {
            get { return _name; }
        }

        public List<Room> Rooms {
            get { return _rooms; }
        }

        public List<Room> roomWithCapacity(int capacity) {
            List<Room> roomsWithCapacity = new List<Room>();
            foreach (Room room in _rooms)
                if (room.Capacity >= capacity)
                    roomsWithCapacity.Add(room);
            return roomsWithCapacity;
        }

        public void addRoom(Room room) {
            _rooms.Add(room);
        }

        public override String ToString() {
            String s = "\tLocation " + _name + ": \n";

            foreach(Room room in _rooms) {
                s += room.ToString();
                s += "\n";
            }

            return s;
        }
    }
}
