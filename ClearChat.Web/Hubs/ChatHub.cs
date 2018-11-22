using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;
using ClearChat.Web.MessageHandling;
using Microsoft.AspNetCore.SignalR;

// ReSharper disable UnusedMember.Global

namespace ClearChat.Web.Hubs
{
    public class ChatHub : Hub, IMessageHub
    {
        private static readonly List<Client> s_Clients = new List<Client>();
        private static readonly IDictionary<string, string> s_ConnectionChannels
            = new ConcurrentDictionary<string, string>();

        private readonly IMessageRepository m_MessageRepository;
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IUserRepository m_UserRepository;
        private readonly IMessageHandler m_MessageHandler;
        private readonly ChatMessageFactory m_ChatMessageFactory;

        public ChatHub(IMessageRepository messageRepository,
                       IConnectionManager connectionManager,
                       IUserRepository userRepository,
                       IMessageHandler messageHandler)
        {
            m_MessageRepository = messageRepository;
            m_ConnectionManager = connectionManager;
            m_UserRepository = userRepository;
            m_MessageHandler = messageHandler;
            m_ChatMessageFactory = new ChatMessageFactory(userRepository);
        }

        public void Send(string message)
        {
            if (!Context.User.Identity.IsAuthenticated)
            {
                Clients.Caller.SendAsync("newMessage",
                    m_ChatMessageFactory.Create("System", "You are not logged in.", "", DateTime.UtcNow));
                return;
            }

            var context = GetContext(message);

            // Commands not yet migrated
            if (message.StartsWith("/"))
            {
                var command = message.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries).First();
                switch (command)
                {
                    case "reset":
                        GetHistory();
                        return;
                    case "purge":
                        var channel = s_ConnectionChannels[Context.ConnectionId];
                        m_MessageRepository.ClearChannel(channel);
                        Clients.Group(channel).SendAsync("initHistory", new ChatMessage[0]);
                        return;
                    case "whoishere":
                        Client[] clients;
                        lock (s_Clients)
                        {
                            clients = s_Clients.ToArray();
                        }
                        var msg = clients.Length == 1 ? "You are alone." : $"{clients.Length} users are here:";
                        PublishSystemMessage(msg, MessageScope.Caller);
                        var sysMessage = m_ChatMessageFactory.Create("System", msg, "", DateTime.UtcNow);
                        Clients.Caller.SendAsync("newMessage", sysMessage);

                        foreach (var client in clients)
                        {
                            PublishSystemMessage(client.Name, MessageScope.Caller);
                        }
                        return;
                }
            }
            m_MessageHandler.Handle(context);
        }

        public void GetHistory()
        {
            var channelName = s_ConnectionChannels[Context.ConnectionId];
            var messages = m_MessageRepository
                           .ChannelMessages(channelName)
                           .Select(m => m_ChatMessageFactory.Create(m.UserId, m.Message, m.ChannelName, m.TimeStampUtc))
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

        public override Task OnConnectedAsync()
        {
            var name = Context.User.Identity.Name;
            s_ConnectionChannels[Context.ConnectionId] = "default";
            Groups.AddToGroupAsync(Context.ConnectionId, "default");
            lock (s_Clients)
            {
                Client client = s_Clients.FirstOrDefault(c => c.Name == name);

                if (client == null)
                {
                    client = new Client(name);
                    s_Clients.Add(client);
                }
                client.AddConnection(Context.ConnectionId);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var name = Context.User.Identity.Name;
            lock (s_Clients)
            {
                var record = s_Clients.First(c => c.Name == name);
                record.RemoveConnection(Context.ConnectionId);
            }
            s_ConnectionChannels.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void Publish(ChatMessage message)
        {
            Clients.Group(message.ChannelName).SendAsync("newMessage", message);
        }

        public void PublishSystemMessage(string message, MessageScope messageScope)
        {
            var msg = m_ChatMessageFactory.Create("System", message, "", DateTime.UtcNow);
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
            Clients.Caller.SendAsync("updateChannelName", channel);
            GetHistory();
        }

        private MessageContext GetContext(string message)
        {
            var user = m_UserRepository.GetUserDetails(Context.User.Identity.Name);
            var channel = s_ConnectionChannels[Context.ConnectionId];
            return new MessageContext(message, user, channel, this, DateTime.UtcNow);
        }
    }

    internal class Client
    {
        public Client(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void AddConnection(string connectionId)
        {
            m_ConnectionIds[connectionId] = null;
        }

        public void RemoveConnection(string connectionId)
        {
            m_ConnectionIds.TryRemove(connectionId, out object _);
        }

        public int ConnectionCount => m_ConnectionIds.Count;

        private readonly ConcurrentDictionary<string, object> m_ConnectionIds
            = new ConcurrentDictionary<string, object>();

    }
}