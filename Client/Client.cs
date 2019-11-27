﻿using System;
using System.Collections.Generic;
using ObjectsLibrary;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using RemotingServicesLibrary;
using System.Runtime.Remoting;
using ClientLibrary;

namespace Client {
    public class Client : ClientAPI {

        //default values
        private String _username = "Rita";
        private String _url = "tcp://localhost:8080/CLIENT";
        private int _port = 8080;
        private String _serverUrl = "tcp://localhost:8086/server1";

        private TcpChannel _channel;

        private ClientService _clientService;
        private IServerService _serverService;

        private IDictionary<String, Meeting> _clientMeetings;           // <Meeting Topic, Meeting>
        private IDictionary<String, IClientService> _otherClients;      // <Client 

        public Client() {
            _clientMeetings = new Dictionary<String, Meeting>();
            _otherClients = new Dictionary<String, IClientService>();

            setClient("CLIENT");
            connectToServer();
        }

        public Client(String username, String clientUrl, String serverUrl) {
            _url = clientUrl;
            String[] clientUrlSplit = clientUrl.Split(new Char[] {':', '/'}, StringSplitOptions.RemoveEmptyEntries);
            _port = Int32.Parse(clientUrlSplit[2]);
            _serverUrl = serverUrl;
            _username = username;
            

            _clientMeetings = new Dictionary<String, Meeting>();
            _otherClients = new Dictionary<String, IClientService>();

            setClient(clientUrlSplit[3]);
            connectToServer();
        }

        public void setClient(String objID) {
            _channel = new TcpChannel(_port);
            ChannelServices.RegisterChannel(_channel, false);

            _clientService = new ClientService(this);
            RemotingServices.Marshal(_clientService, objID, typeof(ClientService));

            Console.WriteLine("[CLIENT:" + _username + "] " + _url + "\n");
        }

        public void connectToServer() {
            _serverService = (IServerService) Activator.GetObject(typeof(IServerService), _serverUrl);
            _serverService.clientConnect(_username, _url);
            getRegisteredClients();

            Console.WriteLine("[CONNECT] Connected to server at "+ _serverUrl);
        }

        public void getRegisteredClients() {
            IDictionary<String, String> registeredClients = _serverService.getRegisteredClients();
            foreach (KeyValuePair<String, String> client in registeredClients) {
                if (client.Key != _username & !_otherClients.ContainsKey(client.Key)) {
                    IClientService clientServ = (IClientService)Activator.GetObject(typeof(IClientService), client.Value);
                    _otherClients.Add(client.Key, clientServ);
                }
            }
        }

        public void listMeetings() {
            Console.WriteLine("|========== Meetings ==========|");

            String list = "";
            List<String> meetingStatusChanged = new List<String>();

            lock (meetingStatusChanged) {
                foreach (Meeting meeting in _clientMeetings.Values) {
                    if (_serverService.checkMeetingStatusChange(meeting)) {
                        meetingStatusChanged.Add(meeting.Topic);
                    }
                }
            }

            lock (_clientMeetings) {
                foreach (String topic in meetingStatusChanged) {
                    _clientMeetings[topic] = _serverService.getMeeting(topic);
                }
            }

            foreach (Meeting meeting in _clientMeetings.Values) {
                list += meeting.ToString(); 
            }
            Console.WriteLine(list);  
        }

        public void createMeeting(String topic, int minAtt, List<Slot> slots) {
            Meeting meeting = _serverService.createMeeting(_username, topic, minAtt, slots);
            if (meeting != null) {
                lock (_clientMeetings) {
                    _clientMeetings.Add(meeting.Topic, meeting);
                }

                Console.WriteLine(meeting.ToString());
                sendInvite(meeting);
            }
            else
                Console.WriteLine("Couldn't create meeting!\nMeeting with topic " + topic + " already exists!");
        }

        public void createMeeting(String topic, int minAtt, List<Slot> slots, List<String> invitees) {
            Meeting meeting = _serverService.createMeeting(_username, topic, minAtt, slots, invitees);
            if (meeting != null) {
                lock (_clientMeetings) {
                    _clientMeetings.Add(meeting.Topic, meeting);
                }

                Console.WriteLine(meeting.ToString());
                sendInvite(meeting);
            }
            else
                Console.WriteLine("Couldn't create meeting!\nMeeting with topic " + topic + " already exists!");
        }

        public void joinMeetingSlot(String topic, String slot){
            Meeting meeting = _serverService.getMeeting(topic);
            if (meeting == null) {
                Console.WriteLine("Couldn't join meeting!\nMeeting " + topic + " doesn't exist!");
                return;
            }
            if (!meeting.checkInvitation(_username)) {
                Console.WriteLine("You were not invited to this meeting!");
            }
            meeting = _serverService.joinMeetingSlot(topic, slot, _username);
            if (meeting != null)
                Console.WriteLine(meeting.ToString());
            else
                Console.WriteLine("Couldn't join meeting!\nDesired slot not found!");
        }

        public void closeMeeting(String topic) {
            Meeting meeting = _serverService.closeMeeting(topic, _username);
            if (meeting == null)
                Console.WriteLine("Couldn't close meeting!\nMinimum number of Atendees not yet reached!");
        }

        public Meeting getMeeting(String topic) {
            return _serverService.getMeeting(topic);
        }

        public void sendInvite(Meeting meeting) {
            getRegisteredClients();
            if (meeting.Invitees == null) {
                foreach (IClientService clientServ in _otherClients.Values)
                    clientServ.receiveInvite(meeting);
            }
            else {
                foreach (String invitee in meeting.Invitees) {  //send to only the invited clients
                    if (_otherClients.ContainsKey(invitee))
                        _otherClients[invitee].receiveInvite(meeting);
                    else
                        Console.WriteLine(invitee + " is not registered in the system.");
                }
            }
        }

        public void receiveInvite(Meeting meeting) {
            Console.WriteLine("[INVITE] Received an invitation to meeting " + meeting.Topic + " from " + meeting.Coord);
            lock (_clientMeetings) {
                _clientMeetings.Add(meeting.Topic, meeting);
            }
        }

        public void wait(int time) {
            System.Threading.Thread.Sleep(time);
        }

        public String status() {
            String s = _username + " known Meetings: \n";
            foreach (Meeting meeting in _clientMeetings.Values) {
                s += meeting.status();
            }
            
            return s;
        }
    }
}
