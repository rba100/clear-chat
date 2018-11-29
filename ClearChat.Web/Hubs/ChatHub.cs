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

        private readonly IMessageRepository m_MessageRepository;
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IUserRepository m_UserRepository;
        private readonly IMessageHandler m_MessageHandler;
        private readonly IChatMessageFactory m_ChatMessageFactory;

        public ChatHub(IMessageRepository messageRepository,
                       IConnectionManager connectionManager,
                       IUserRepository userRepository,
                       IMessageHandler messageHandler,
                       IChatMessageFactory chatMessageFactory)
        {
            m_MessageRepository = messageRepository;
            m_ConnectionManager = connectionManager;
            m_UserRepository = userRepository;
            m_MessageHandler = messageHandler;
            m_ChatMessageFactory = chatMessageFactory;
        }

        public void Send(SendEventBinding eventBinding)
        {
            var channelName = eventBinding.Channel;
            var message = eventBinding.Body;
            if (!Context.User.Identity.IsAuthenticated)
            {
                Clients.Caller.SendAsync("newMessage",
                    m_ChatMessageFactory.Create("System", "You are not logged in.", "", DateTime.UtcNow));
                return;
            }

            var context = GetContext(message, channelName);

            // Commands not yet migrated
            if (message.StartsWith("/"))
            {
                var command = message.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries).First();
                switch (command)
                {
                    case "whoishere":
                        Client[] clients;
                        lock (s_Clients)
                        {
                            clients = s_Clients.Where(c => c.ConnectionCount > 0).ToArray();
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

        public void GetHistory(string channelName)
        {
            if (channelName != "default" &&
                !m_MessageRepository.GetChannelMembershipsForUser(Context.User.Identity.Name).Contains(channelName))
            {
                return;
            }
            var messages = m_MessageRepository
                           .ChannelMessages(channelName)
                           .Select(m => m_ChatMessageFactory.Create(m.UserId, m.Message, m.ChannelName, m.TimeStampUtc))
                           .OrderBy(m => m.TimeStampUtc);
            Clients.Caller.SendAsync("initHistory", channelName, messages);
        }

        public void GetChannels()
        {
            if (!Context.User.Identity.IsAuthenticated)
            {
                return;
            }

            var userId = Context.User.Identity.Name;
            var channelNames = new[] { "default" }.Concat(m_MessageRepository.GetChannelMembershipsForUser(userId));
            foreach (var channelName in channelNames)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, channelName);
            }
            Clients.Caller.SendAsync("channelMembership", channelNames);
        }

        public override Task OnConnectedAsync()
        {
            var name = Context.User.Identity.IsAuthenticated ? Context.User.Identity.Name : null;

            if (name != null)
            {
                m_ConnectionManager.RegisterConnection(Context.ConnectionId, name);
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
            m_ConnectionManager.RegisterDisconnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public void Publish(ChatMessage message)
        {
            Clients.Group(message.ChannelName).SendAsync("newMessage", message);
        }

        public void PublishSystemMessage(string message, MessageScope messageScope)
        {
            var msg = m_ChatMessageFactory.Create("System", message, "system", DateTime.UtcNow);
            if (messageScope == MessageScope.All)
                Clients.All.SendAsync("newMessage", msg);
            else
                Clients.Caller.SendAsync("newMessage", msg);
        }

        public void UpdateChannelMembership(string connectionId)
        {
            var userId = Context.User.Identity.Name;
            var channels = m_MessageRepository.GetChannelMembershipsForUser(userId);
            var channelsAndDefault = new[] { "default" }.Concat(channels).ToArray();
            foreach (var channelName in channelsAndDefault)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, channelName);
            }
            Clients.Client(connectionId).SendAsync("channelMembership", channelsAndDefault);
        }

        public void RemoveChannelMembership(string connectionId, string channelName)
        {
            Groups.RemoveFromGroupAsync(connectionId, channelName);
        }

        public void ForceInitHistory(string connectionId, string channelName)
        {
            var messages = m_MessageRepository
                           .ChannelMessages(channelName)
                           .Select(m => m_ChatMessageFactory.Create(m.UserId, m.Message, m.ChannelName, m.TimeStampUtc))
                           .OrderBy(m => m.TimeStampUtc);
            Clients.Client(connectionId).SendAsync("initHistory", channelName, messages);
        }

        private MessageContext GetContext(string message, string channelName)
        {
            var user = m_UserRepository.GetUserDetails(Context.User.Identity.Name);
            return new MessageContext(message, user, channelName, this, DateTime.UtcNow);
        }
    }

    public class SendEventBinding
    {
        public string Channel { get; set; }
        public string Body { get; set; }
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