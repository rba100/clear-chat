using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

using Microsoft.AspNetCore.SignalR;

// ReSharper disable UnusedMember.Global

namespace ClearChat.Web.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly List<Client> s_Clients = new List<Client>();

        private readonly IMessageRepository m_MessageRepository;

        private MessageFactory m_MessageFactory = new MessageFactory();

        public ChatHub(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public void Send(string message)
        {
            if (!Context.User.Identity.IsAuthenticated)
            {
                Clients.Caller.SendAsync("newMessage", 
                    m_MessageFactory.Create("System", "You are not logged in.", DateTime.UtcNow));
                return;
            }

            if (message.StartsWith("/"))
            {
                var command = message.Substring(1);
                switch (command)
                {
                    case "clear":
                        m_MessageRepository.ClearChannel("default");
                        Clients.All.SendAsync("initHistory", new ChatMessage[0]);
                        break;

                    case "whoishere":
                        Client[] clients;
                        lock (s_Clients)
                        {
                            clients = s_Clients.ToArray();
                        }
                        var msg = clients.Length == 1 ? "You are alone." : $"{clients.Length} users are here:";
                        var sysMessage = m_MessageFactory.Create("System", msg, DateTime.UtcNow);
                        Clients.Caller.SendAsync("newMessage", sysMessage);

                        foreach (var client in clients)
                        {
                            var clientReport = m_MessageFactory.Create("System", client.Name, DateTime.UtcNow);
                            Clients.Caller.SendAsync("newMessage", clientReport);
                        }
                        break;
                    case "colours":
                        for (int i = 0; i < 255; i++)
                        {
                            var clientReport = m_MessageFactory.Create("Test" + i, "TEST", DateTime.UtcNow.AddTicks(i));
                            Clients.Caller.SendAsync("newMessage", clientReport);
                        }

                        break;
                    default:
                        var messageItem = m_MessageFactory.Create("System", "Unrecognised command: " + command, DateTime.UtcNow);
                        Clients.Caller.SendAsync("newMessage", messageItem);
                        break;
                }
            }
            else
            {
                var messageItem = m_MessageFactory.Create(Context.User.Identity.Name, message, DateTime.UtcNow);
                m_MessageRepository.WriteMessage(messageItem);
                Clients.All.SendAsync("newMessage", messageItem);

                if (message.ToLower().Contains("spaz"))
                {
                    var msg = m_MessageFactory.Create(
                        "SjBot",
                        $"I'm watching you, {Context.User.Identity.Name}!", 
                        DateTime.UtcNow);
                    Clients.All.SendAsync("newMessage", msg);
                }
            }
        }

        public void GetHistory()
        {
            var messages = m_MessageRepository.ChannelMessages("default")
                                              .OrderBy(m => m.TimeStampUtc);
            Clients.Caller.SendAsync("initHistory", messages);
        }

        public void GetClients()
        {
            lock (s_Clients)
            {
                var items = s_Clients.ToArray();
                Clients.Caller.SendAsync("initClients", items);
            }
        }

        private void PushUpdateClients()
        {
            var items = s_Clients.ToArray();
            Clients.All.SendAsync("initClients", items);
        }

        public override Task OnConnectedAsync()
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
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var name = Context.User.Identity.Name;
            lock (s_Clients)
            {
                var record = s_Clients.First(c => c.Name == name);
                record.ConnectionCount--;
                if (record.ConnectionCount == 0) s_Clients.Remove(record);
                PushUpdateClients();
            }
            return base.OnDisconnectedAsync(exception);
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
}