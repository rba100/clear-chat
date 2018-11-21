using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;
using ClearChat.Web.SlashCommands;
using Microsoft.AspNetCore.SignalR;

// ReSharper disable UnusedMember.Global

namespace ClearChat.Web.Hubs
{
    public class ChatHub : Hub, IMessageSink
    {
        private static readonly List<Client> s_Clients = new List<Client>();
        private static readonly IDictionary<string, string> s_ConnectionChannels
            = new ConcurrentDictionary<string, string>();

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
                    m_MessageFactory.Create("System", "You are not logged in.", "", DateTime.UtcNow));
                return;
            }

            var user = m_UserRepository.GetUserDetails(Context.User.Identity.Name);

            if (message.StartsWith("/"))
            {
                var command = message.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries).First();
                switch (command)
                {
                    case "reset":
                        GetHistory();
                        break;
                    case "purge":
                        var channel = s_ConnectionChannels[Context.ConnectionId];
                        m_MessageRepository.ClearChannel(channel);
                        Clients.Group(channel).SendAsync("initHistory", new ChatMessage[0]);
                        break;
                    case "whoishere":
                        Client[] clients;
                        lock (s_Clients)
                        {
                            clients = s_Clients.ToArray();
                        }
                        var msg = clients.Length == 1 ? "You are alone." : $"{clients.Length} users are here:";
                        PublishSystemMessage(msg, MessageScope.Caller);
                        var sysMessage = m_MessageFactory.Create("System", msg, "", DateTime.UtcNow);
                        Clients.Caller.SendAsync("newMessage", sysMessage);

                        foreach (var client in clients)
                        {
                            PublishSystemMessage(client.Name, MessageScope.Caller);
                        }
                        break;
                    default:
                        m_SlashCommandHandler.Handle(user, this, message);
                        break;
                }
            }
            else
            {
                var channel = s_ConnectionChannels[Context.ConnectionId];
                var messageItem = m_MessageFactory.Create(Context.User.Identity.Name,
                                                          message,
                                                          channel,
                                                          DateTime.UtcNow);
                m_MessageRepository.WriteMessage(messageItem);
                Clients.Group(channel).SendAsync("newMessage", messageItem);

                if (message.ToLower().Contains("spaz"))
                {
                    var msg = m_MessageFactory.Create(
                        "SjBot",
                        $"I'm watching you, {Context.User.Identity.Name}!",
                        channel,
                        DateTime.UtcNow);
                    Clients.Group(channel).SendAsync("newMessage", msg);
                }
            }
        }

        public void GetHistory()
        {
            var channel = s_ConnectionChannels[Context.ConnectionId];
            RefreshHistory(channel);
        }

        private void RefreshHistory(string channelName)
        {
            var messages = m_MessageRepository
                           .ChannelMessages(channelName)
                           .Select(m => m_MessageFactory.Create(m.UserId, m.Message, m.ChannelName, m.TimeStampUtc))
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
            s_ConnectionChannels[Context.ConnectionId] = "default";

            Groups.AddToGroupAsync(Context.ConnectionId, "default");
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
            s_ConnectionChannels.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void Publish(ChatMessage message)
        {
            Clients.All.SendAsync("newMessage", message);
        }

        public void PublishSystemMessage(string message, MessageScope messageScope)
        {
            var msg = m_MessageFactory.Create("System", message, "", DateTime.UtcNow);
            if (messageScope == MessageScope.All)
                Clients.All.SendAsync("newMessage", msg);
            else
                Clients.Caller.SendAsync("newMessage", msg);
        }

        public void ChangeChannel(string channel)
        {
            var previousChannel = s_ConnectionChannels[Context.ConnectionId];
            Groups.RemoveFromGroupAsync(Context.ConnectionId, previousChannel);
            Groups.AddToGroupAsync(Context.ConnectionId, channel);
            s_ConnectionChannels[Context.ConnectionId] = channel;
            GetHistory();
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