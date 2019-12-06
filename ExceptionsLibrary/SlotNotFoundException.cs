using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionsLibrary {

    [Serializable]
    public class SlotNotFoundException : ApplicationException {
        public String message;

        public SlotNotFoundException(String message) {
            this.message = message;
        }

        public SlotNotFoundException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) {
            message = info.GetString("message");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("message", message);
        }
    }
}
