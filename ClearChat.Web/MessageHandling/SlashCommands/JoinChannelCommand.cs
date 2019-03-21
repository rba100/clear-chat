using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    class JoinChannelCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;
        private readonly IConnectionManager m_ConnectionManager;

        public JoinChannelCommand(IMessageRepository messageRepository,
                                  IConnectionManager connectionManager)
        {
            m_MessageRepository = messageRepository;
            m_ConnectionManager = connectionManager;
        }

        public string CommandText => "join";

        public void Handle(MessageContext context, string arguments)
        {
            var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!parts.Any())
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: correct usage is /join channelName channelPassword");
                return;
            }

            var channelName = parts[0];

            if (channelName.StartsWith("@"))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: channel names cannot start with '@'. This is a reserved character for direct messages.");
                return;
            }

            if (channelName.Length > 20)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: channel names should be 20 characters or fewer.");
                return;
            }

            var channels = m_MessageRepository.GetChannelMembershipsForUser(context.User.Id);
            if (channels.Contains(channelName))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: you are already a member of that channel.");
                return;
            }

            var password = parts.Length > 1 ? parts[1] : string.Empty;
            var channelResult = m_MessageRepository.GetOrCreateChannel(channelName, password);
            var connectionIds = m_ConnectionManager.GetConnectionsForUser(context.User);
            switch (channelResult)
            {
                case SwitchChannelResult.Denied:
                    context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: wrong password channel");
                    break;
                case SwitchChannelResult.Created:
                    m_MessageRepository.AddChannelMembership(context.User.Id, channelName);
                    foreach(var connectionId in connectionIds) context.MessageHub.UpdateChannelMembership(connectionId);
                    context.MessageHub.PublishSystemMessage(context.ConnectionId, $"You created channel '{channelName}'");
                    break;
                case SwitchChannelResult.Accepted:
                    m_MessageRepository.AddChannelMembership(context.User.Id, channelName);
                    foreach (var connectionId in connectionIds) context.MessageHub.UpdateChannelMembership(connectionId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string HelpText => "Join or create a channel with: /join channelName [password]";
    }
}