using System;
using RemotingServicesLibrary;
using ObjectsLibrary;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace Server {
    public class ServerService : MarshalByRefObject, IServerService {

        private Server _server;
     
        public ServerService(Server server) {
            _server = server;
        }

        public Boolean checkFreeze() {
            return _server.Freeze;
        }

        public void clientConnect(String username, String clientUrl) {
            _server.clientConnect(username, clientUrl);
        }

        public void receiveNewClient(String username, String clientUrl) {
            _server.receiveNewClient(username, clientUrl);
        }

        public IDictionary<String, String> getRegisteredClients() {
            return _server.Clients;
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots) {
            if (_server.Freeze) {
                //_server.DelayedMessages(meeting)
            }
            return _server.createMeeting(username, topic, minAtt, slots);
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            if (_server.Freeze) {
                //_server.DelayedMessages(meeting)
            }
            return _server.createMeeting(username, topic, minAtt, slots, invitees);
        }

        public Meeting getMeeting(String topic) {
            return _server.getMeeting(topic);
        }

        public bool checkMeetingStatusChange(Meeting meeting) {
            return _server.checkMeetingStatusChange(meeting);
        }

        public Meeting joinMeetingSlot(String topic, String slot, String username) {
            return _server.joinMeetingSlot(topic, slot, username);
        }

        public void closeMeeting(String topic) {
            _server.closeMeeting(topic);
        }

        public void receiveMeeting(IDictionary<String, int> vectorTimeStamp, IDictionary<String, Meeting> meetings) {
            _server.receiveMeeting(vectorTimeStamp, meetings);
        }

        public void addRoom(String roomLocation, int capacity, String name) {
            _server.addRoom(roomLocation, capacity, name);
        }

        public void printStatus() {
            _server.printStatus();
        }
    }
}
