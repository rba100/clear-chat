using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using ClearChat.Models;
using ClearChat.Repositories;
using Microsoft.AspNet.SignalR;

// ReSharper disable UnusedMember.Global

namespace ClearChat.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly List<Client> s_Clients = new List<Client>();

        private readonly IMessageRepository m_MessageRepository;

        public ChatHub(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public void Send(string message)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                if (message.StartsWith("/"))
                {
                    var command = message.Substring(1);
                    switch (command)
                    {
                        case "clear":
                            m_MessageRepository.ClearChannel("default");
                            Clients.All.initHistory(new MessageItem[0]);
                            break;

                        case "whoishere":
                            Client[] clients;
                            lock (s_Clients)
                            {
                                clients = s_Clients.ToArray();
                            }
                            var msg = clients.Length == 1 ? "You are alone." : $"{clients.Length} users are here:";
                            Clients.Caller.newMessage(new MessageItem("System", msg, DateTime.UtcNow));

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
                    m_MessageRepository.WriteMessage(ToOther(messageItem));
                    Clients.All.newMessage(messageItem);

                    if (message.ToLower().Contains("spaz"))
                    {
                        Clients.All.newMessage(new MessageItem("WiseBot", $"I'm watching you, {Context.User.Identity.Name}!", DateTime.UtcNow));
                    }
                }
            }
        }

        ChatMessage ToOther(MessageItem item)
        {
            return new ChatMessage(item.Name, "default", item.Message, item.TimeStamp);
        }

        MessageItem ToOther(ChatMessage item)
        {
            return new MessageItem(item.UserId, item.Message, item.TimeStampUtc);
        }

        public void GetHistory()
        {
            var messages = m_MessageRepository.ChannelMessages("default")
                                              .Select(ToOther)
                                              .OrderBy(m => m.TimeStamp);
            Clients.Caller.initHistory(messages);
        }

        public void GetClients()
        {
            lock (s_Clients)
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