using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    class LeaveChannelCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;
        private readonly IConnectionManager m_ConnectionManager;

        public LeaveChannelCommand(IMessageRepository messageRepository, IConnectionManager connectionManager)
        {
            m_MessageRepository = messageRepository;
            m_ConnectionManager = connectionManager;
        }

        public string CommandText => "leave";

        public void Handle(MessageContext context, string arguments)
        {
            var userId = context.UserId;
            var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!parts.Any())
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: correct usage is /leave channelName");
                return;
            }

            var channelName = parts[0];

            var channels = m_MessageRepository.GetChannelMembershipsForUser(userId);
            if (!channels.Contains(channelName))
            {
                return;
            }

            m_MessageRepository.RemoveChannelMembership(userId, channelName);
            var connectionIds = m_ConnectionManager.GetConnectionsForUser(userId);
            foreach (var connection in connectionIds)
            {
                context.MessageHub.RemoveChannelMembership(connection, channelName);
                context.MessageHub.UpdateChannelMembership(connection);
            }
        }

        public string HelpText => "Leave a channel with: /leave channelName";
    }
}