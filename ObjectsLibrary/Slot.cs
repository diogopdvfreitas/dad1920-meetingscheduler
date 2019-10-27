using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsLibrary {

    public class Slot {
        private Location _location;
        private String _date;

        public Slot(Location location, String date) {
            _location = location;
            _date = date;
        }

        public Location Location {
            get { return _location; }
            set { _location = value; }
        }

        public String Date {
            get { return _date; }
            set { _date = value; }
        }

        public override String ToString() {
            return _location + "," + _date;
        }
    }
}
