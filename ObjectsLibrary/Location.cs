using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsLibrary {
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
    }
}
