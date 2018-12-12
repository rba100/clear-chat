using System;
using System.Linq;
using System.Text.RegularExpressions;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    class InviteSlashCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;
        private readonly IConnectionManager m_ConnectionManager;

        public InviteSlashCommand(IMessageRepository messageRepository, IConnectionManager connectionManager)
        {
            m_MessageRepository = messageRepository;
            m_ConnectionManager = connectionManager;
        }

        public string CommandText => "invite";

        public void Handle(MessageContext context, string arguments)
        {
            var intiverUserId = context.UserId;
            var parts = SplitParameters(arguments);
            if (parts.Length != 2)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: correct usage is /invite userId channelName");
                return;
            }

            var inviteeUserId = parts[0];
            var channelName = parts[1];

            if (channelName.StartsWith("@"))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: channel names cannot start with '@'. This is a reserved character for direct messages.");
                return;
            }

            var inviterChannels = m_MessageRepository.GetChannelMembershipsForUser(intiverUserId);
            var inviteeChannels = m_MessageRepository.GetChannelMembershipsForUser(inviteeUserId);

            if (!inviterChannels.Contains(channelName))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId,
                                                        "Error: you're not a member of that channel yourself, or it doesn't exist.");
                return;
            }

            if (inviteeChannels.Contains(channelName))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: that user is already a member of that channel.");
                return;
            }

            m_MessageRepository.AddChannelMembership(inviteeUserId, channelName);
            var connectionIds = m_ConnectionManager.GetConnectionsForUser(inviteeUserId);
            foreach (var connectionId in connectionIds) context.MessageHub.UpdateChannelMembership(connectionId);
            context.MessageHub.PublishSystemMessage(context.ConnectionId,
                                                    $"{inviteeUserId} is now a member of {channelName}.");

        }
        public string HelpText => "adds another user to a channel. /invite \"userId\" \"channelName\"";
        
        private static string[] SplitParameters(string input)
        {
            return Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
            .Select(m => m.Value.Trim('\"'))
            .ToArray();
        }
    }
}