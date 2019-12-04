using System;

namespace ExceptionsLibrary
{
    [Serializable]
    public class ClosedMeetingException : ApplicationException{
        
        public ClosedMeetingException() {
        }

        public ClosedMeetingException(String message) : base(message) {
        }
    }
}
