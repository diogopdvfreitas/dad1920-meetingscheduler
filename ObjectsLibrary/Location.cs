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

        public void addRoom(Room room) {
            _rooms.Add(room);
        }

        public List<Room> getRoomsWithEnoughCapacity(int minCapacity) {
            List<Room> rooms = new List<Room>();
            foreach(Room room in _rooms) {
                if(room.Capacity >= minCapacity) {
                    rooms.Add(room);
                }
            }
            return rooms;
        }

    }
}
