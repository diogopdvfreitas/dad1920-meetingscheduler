using System;
using System.Collections;

namespace Client {
    [Serializable]
    public class Meeting {
        String _coord;
        String _topic;
        int _nPart;
        int _nSlots;
        int _nInvit;
        ArrayList _slots;
        ArrayList _invits;

        public Meeting(String topic, int nPart, int nSlots, int nInvit, ArrayList slots, ArrayList invits) {
            //_coord = coord;
            _topic = topic;
            _nPart = nPart;
            _nSlots = nSlots;
            _nInvit = nInvit;
            _slots = slots;
            _invits = invits;
        }

    }
}
