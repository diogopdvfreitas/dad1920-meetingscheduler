using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionsLibrary {
    public class NotEnoughAttendeesExceptions :ApplicationException {
        public NotEnoughAttendeesExceptions() {
        }

        public NotEnoughAttendeesExceptions(String message) : base(message) {
        }
    }
}
