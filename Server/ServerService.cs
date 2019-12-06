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

        public IDictionary<String, int> clientConnect(String username, String clientUrl) {
            return getServer().clientConnect(username, clientUrl);
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


        public IDictionary<String, int> getVectorClock() {
            return getServer().VectorClock;
        }

        public MeetingMessage createMeeting(String username, String topic, int minAtt, List<Slot> slots) {
            return getServer().createMeeting(username, topic, minAtt, slots);
        }

        public MeetingMessage createMeeting(String username, String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            return getServer().createMeeting(username, topic, minAtt, slots, invitees);
        }

        public MeetingMessage joinMeetingSlot(String topic, String slot, String username) {
            return getServer().joinMeetingSlot(topic, slot, username);
        }

        public MeetingMessage closeMeeting(String topic, String username){
            return getServer().closeMeeting(topic, username);
        }
        
        public Meeting getMeeting(String topic) {
            return getServer().getMeeting(topic);
        }

        public bool checkMeetingStatusChange(Meeting meeting) {
            return getServer().checkMeetingStatusChange(meeting);
        }
       
        public void receiveChanges(String serverUrl, IDictionary<String, int> vectorClock, IDictionary<String, List<Meeting>> meetings) {
            getServer().receiveChanges(serverUrl, vectorClock, meetings);
        }

        public void addRoom(String roomLocation, int capacity, String name) {
            getServer().addRoom(roomLocation, capacity, name);
        }

        public void addLocation(String location_name, Location location) {
            getServer().addLocation(location_name, location);
        }

        public IDictionary<String, int> updateServer() {
            return getServer().updateServer();
        }

        public void getUpdatedMeetingsFromUpdatedServer(String requestingServerURL, IDictionary<String, int> vectorClock) {
            getServer().getUpdatedMeetingsFromUpdatedServer(requestingServerURL, vectorClock);
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


        public KeyValuePair<int, String> grantCloseTicket(String serverUrl) {
            return getServer().grantCloseTicket(serverUrl);
        }

        public void newGrantedTicket(String leader, KeyValuePair<int, String> newGrantedTicketByLeader) {
            getServer().newGrantedTicket(leader, newGrantedTicketByLeader);
        }

        public void selectNewLeader() {
            getServer().selectNewLeader();
        }
    }
}
