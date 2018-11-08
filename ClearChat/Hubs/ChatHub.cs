using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

// ReSharper disable UnusedMember.Global

namespace ClearChat.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly List<MessageItem> s_ChatHistory = new List<MessageItem>();
        private static readonly List<Client> s_Clients = new List<Client>();

        public void Send(string message)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                if (message.StartsWith("/"))
                {
                    var command = message.Substring(1);
                    switch (command)
                    {
                        case "clear": lock(s_ChatHistory) s_ChatHistory.Clear(); Clients.All.initHistory(new object[0]);
                            break;

                        case "whoishere":
                            Client[] clients;
                            lock (s_Clients)
                            {
                                clients = s_Clients.ToArray();
                            }
                            var plural = clients.Length == 1 ? "" : "s";
                            Clients.Caller.newMessage(new MessageItem("System", $"{clients.Length} user{plural} are here:", DateTime.UtcNow));

                            foreach (var client in clients)
                            {
                                var clientReport = new MessageItem("System", client.Name, DateTime.UtcNow);
                                Clients.Caller.newMessage(clientReport);
                            }
                            break;
                        default:
                            var messageItem = new MessageItem("System", "Unrecognised command: " + command, DateTime.UtcNow);
                            Clients.Caller.newMessage(messageItem);
                            break;
                    }
                }
                else
                {
                    var messageItem = new MessageItem(Context.User.Identity.Name, message, DateTime.UtcNow);
                    
                    Clients.All.newMessage(messageItem);

                    if (message.ToLower().Contains("spaz"))
                    {
                        Clients.All.newMessage(new MessageItem("WiseBot", $"I'm watching you, {Context.User.Identity.Name}!", DateTime.UtcNow));
                    }

                    lock (s_ChatHistory)
                    {
                        s_ChatHistory.Add(messageItem);
                        if (s_ChatHistory.Count > 400) s_ChatHistory.RemoveAt(0);
                    }
                }
            }
        }

        public void GetHistory()
        {
            lock (s_ChatHistory)
            {
                var items = s_ChatHistory.ToArray();
                Clients.Caller.initHistory(items);
            }
        }

        public void GetClients()
        {
            lock (s_ChatHistory)
            {
                var items = s_Clients.ToArray();
                Clients.Caller.initClients(items);
            }
        }

        private void PushUpdateClients()
        {
            var items = s_Clients.ToArray();
            Clients.All.initClients(items);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var name = Context.User.Identity.Name;
            lock (s_Clients)
            {
                var record = s_Clients.First(c => c.Name == name);
                record.ConnectionCount--;
                if (record.ConnectionCount == 0) s_Clients.Remove(record);
                PushUpdateClients();
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnConnected()
        {
            var name = Context.User.Identity.Name;
            lock (s_Clients)
            {
                if (s_Clients.All(c => c.Name != name))
                {
                    s_Clients.Add(new Client(name) { ConnectionCount = 1 });
                }
                else s_Clients.First(c => c.Name == name).ConnectionCount++;
                PushUpdateClients();
            }
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            var name = Context.User.Identity.Name;
            lock (s_Clients)
            {
                if (s_Clients.All(c => c.Name != name))
                {
                    s_Clients.Add(new Client(name) { ConnectionCount = 1 });
                    PushUpdateClients();
                }
                else s_Clients.First(c => c.Name == name).ConnectionCount++;
                PushUpdateClients();
            }
            return base.OnReconnected();
        }
    }

    internal class Client
    {
        public Client(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public int ConnectionCount { get; set; }
    }

    public class MessageItem
    {
        public MessageItem(string name, string message, DateTime timeStamp)
        {
            Name = name;
            Message = message;
            TimeStamp = timeStamp;
        }

        public string Name { get; }
        public string Message { get; }
        public DateTime TimeStamp { get; }
    }
}