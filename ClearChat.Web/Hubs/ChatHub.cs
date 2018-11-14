using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;
using ClearChat.Web.Messaging;
using ClearChat.Web.SlashCommands;
using Microsoft.AspNetCore.SignalR;

// ReSharper disable UnusedMember.Global

namespace ClearChat.Web.Hubs
{
    public class ChatHub : Hub, IMessageSink
    {
        private static readonly List<Client> s_Clients = new List<Client>();

        private readonly IMessageRepository m_MessageRepository;
        private readonly IUserRepository m_UserRepository;
        private readonly ISlashCommandHandler m_SlashCommandHandler;
        private readonly MessageFactory m_MessageFactory;

        public ChatHub(IMessageRepository messageRepository, 
                       IUserRepository userRepository,
                       ISlashCommandHandler slashCommandHandler)
        {
            m_MessageRepository = messageRepository;
            m_UserRepository = userRepository;
            m_SlashCommandHandler = slashCommandHandler;
            m_MessageFactory = new MessageFactory(userRepository);
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
                var command = message.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries).First();
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
                    case "colour":
                        var colour = message.Substring("colour".Length + 1).Trim();
                        if (colour.Length != 6 && colour.Length != 7)
                        {
                            var clientReport = m_MessageFactory.Create("System", "Not a valid colour. Must be six character RGB hex code, with optional hash prefix.", DateTime.UtcNow);
                            Clients.Caller.SendAsync("newMessage", clientReport);
                        }

                        var newColour = new string(colour.Reverse().Take(6).Reverse().ToArray());
                        m_UserRepository.UpdateUserDetails(new User(Context.User.Identity.Name, newColour));

                        break;
                    default:
                        m_SlashCommandHandler.Handle(this, message);
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
            var messages = m_MessageRepository
                .ChannelMessages("default")
                .Select(m => m_MessageFactory.Create(m.UserId, m.Message, m.TimeStampUtc))
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

        public void Publish(ChatMessage message)
        {
            Clients.All.SendAsync("newMessage", message);
        }

        public void PublishSystemMessage(string message, string channelId, MessageScope messageScope)
        {
            var msg = m_MessageFactory.Create("System", message, DateTime.UtcNow);
            if(messageScope == MessageScope.All)
                Clients.All.SendAsync("newMessage", msg);
            else
                Clients.Caller.SendAsync("newMessage", msg);
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