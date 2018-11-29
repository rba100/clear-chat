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
            var userId = context.User.UserId;
            var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!parts.Any())
            {
                context.MessageHub.PublishSystemMessage("Error: correct usage is /join channelName channelPassword",
                                                        MessageScope.Caller);
                return;
            }

            var channelName = parts[0];

            if (channelName.StartsWith("@"))
            {
                context.MessageHub.PublishSystemMessage("Error: channel names cannot start with '@'. This is a reserved character for direct messages.",
                                                        MessageScope.Caller);
                return;
            }

            if (channelName.Length > 20)
            {
                context.MessageHub.PublishSystemMessage("Error: channel names should be 20 characters or fewer.",
                                                        MessageScope.Caller);
                return;
            }

            var channels = m_MessageRepository.GetChannelMembershipsForUser(userId);
            if (channels.Contains(channelName))
            {
                context.MessageHub.PublishSystemMessage("Error: you are already a member of that channel.",
                                                        MessageScope.Caller);
                return;
            }

            var password = parts.Length > 1 ? parts[1] : string.Empty;
            var channelResult = m_MessageRepository.GetOrCreateChannel(channelName, password);
            var connectionIds = m_ConnectionManager.GetConnectionsForUser(userId);
            switch (channelResult)
            {
                case SwitchChannelResult.Denied:
                    context.MessageHub.PublishSystemMessage("Error: wrong password channel",
                                                            MessageScope.Caller);
                    break;
                case SwitchChannelResult.Created:
                    m_MessageRepository.AddChannelMembership(userId, channelName);
                    foreach(var connectionId in connectionIds) context.MessageHub.UpdateChannelMembership(connectionId);
                    context.MessageHub.PublishSystemMessage($"You created channel '{channelName}'",
                                                            MessageScope.Caller);
                    break;
                case SwitchChannelResult.Accepted:
                    m_MessageRepository.AddChannelMembership(userId, channelName);
                    foreach (var connectionId in connectionIds) context.MessageHub.UpdateChannelMembership(connectionId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string HelpText => "Join or create a channel with: /join channelName [password]";
    }
}