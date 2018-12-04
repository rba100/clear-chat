using System;
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
    public class ChatHub : Hub
    {
        private readonly IMessageRepository m_MessageRepository;
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IMessageHandler m_MessageHandler;
        private readonly IMessageHub m_MessageHub;

        public ChatHub(IMessageRepository messageRepository,
                       IConnectionManager connectionManager,
                       IMessageHandler messageHandler,
                       IMessageHub messageHub)
        {
            m_MessageRepository = messageRepository;
            m_ConnectionManager = connectionManager;
            m_MessageHandler = messageHandler;
            m_MessageHub = messageHub;
        }

        public void Send(SendEventBinding eventBinding)
        {
            if (!Context.User.Identity.IsAuthenticated)
            {
                m_MessageHub.PublishSystemMessage(Context.ConnectionId, "You are not logged in.");
                return;
            }
            m_MessageHandler.Handle(GetContext(eventBinding.Body, eventBinding.Channel));
        }

        public void GetHistory(string channelName)
        {
            if (channelName != "default" &&
                !m_MessageRepository.GetChannelMembershipsForUser(Context.User.Identity.Name).Contains(channelName))
            {
                return;
            }
            m_MessageHub.SendChannelHistory(Context.ConnectionId, channelName);
        }

        public void GetUserDetails(string userId)
        {
            m_MessageHub.PublishUserDetails(Context.ConnectionId, new[] { userId });
        }

        public void GetChannels()
        {
            if (!Context.User.Identity.IsAuthenticated)
            {
                return;
            }
            m_MessageHub.UpdateChannelMembership(Context.ConnectionId);
        }

        public void Typing(string channelName)
        {
            Clients.Group(channelName).SendAsync("isTyping", Context.User.Identity.Name, channelName);
        }

        public override Task OnConnectedAsync()
        {
            var name = Context.User.Identity.IsAuthenticated ? Context.User.Identity.Name : null;

            if (name != null)
            {
                m_ConnectionManager.RegisterConnection(Context.ConnectionId, name);
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            m_ConnectionManager.RegisterDisconnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        private MessageContext GetContext(string message, string channelName)
        {
            return new MessageContext(message,
                                      Context.User.Identity.Name,
                                      Context.ConnectionId,
                                      channelName,
                                      m_MessageHub,
                                      DateTime.UtcNow);
        }
    }

    public class SendEventBinding
    {
        public string Channel { get; set; }
        public string Body { get; set; }
    }
}