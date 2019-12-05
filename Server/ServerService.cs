using System;
using RemotingServicesLibrary;
using ObjectsLibrary;
using System.Collections.Generic;
using ExceptionsLibrary;
using System.Net.Sockets;

namespace Server {
    public class ServerService : MarshalByRefObject, IServerService {

        private Server _server;
     
        public ServerService(Server server) {
            _server = server;
        }

        public Server getServer() {
            _server.checkDelay();
            return _server;
        }

        public void clientConnect(String username, String clientUrl) {
            getServer().clientConnect(username, clientUrl);
        }

        public void receiveNewClient(String username, String clientUrl) {
            getServer().receiveNewClient(username, clientUrl);
        }

        public IDictionary<String, String> getRegisteredClients() {
            return getServer().Clients;
        }

        public IDictionary<String, IServerService> getRegisteredServers() {
            return getServer().Servers;
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots) {
            return getServer().createMeeting(username, topic, minAtt, slots);
        }

        public Meeting createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            return getServer().createMeeting(username, topic, minAtt, slots, invitees);
        }

        public Meeting joinMeetingSlot(String topic, String slot, String username) {
            return getServer().joinMeetingSlot(topic, slot, username);
        }

        public Meeting closeMeeting(String topic, String username){
            return getServer().closeMeeting(topic, username);
        }
        
        public Meeting getMeeting(String topic) {
            return getServer().getMeeting(topic);
        }

        public bool checkMeetingStatusChange(Meeting meeting) {
            return getServer().checkMeetingStatusChange(meeting);
        }
       
        public void receiveChanges(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings, int serverCloseTicket) {
            getServer().receiveChanges(serverUrl, vectorClock, meetings, serverCloseTicket);
        }

        public void addRoom(String roomLocation, int capacity, String name) {
            getServer().addRoom(roomLocation, capacity, name);
        }

        public void addLocation(String location_name, Location location) {
            getServer().addLocation(location_name, location);
        }

        public String status() {
            return getServer().status();
        }
        
        public void freeze() {
            getServer().freeze();
        }

        public void unfreeze() {
            getServer().unfreeze();
        }

        public int grantCloseTicket(String serverUrl) {
            return getServer().grantCloseTicket(serverUrl);
        }
    }
}
