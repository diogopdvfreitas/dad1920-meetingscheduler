using System;

namespace ExceptionsLibrary
{
    [Serializable]
    public class ClosedMeetingException : ApplicationException{
        public String message;
        
        public ClosedMeetingException(String message) {
            this.message = message;
        }

        public ClosedMeetingException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) {
            message = info.GetString("message");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("message", message);
        }
    }
}
