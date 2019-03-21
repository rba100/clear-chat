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
        private readonly IUserRepository m_UserRepository;
        private readonly IConnectionManager m_ConnectionManager;

        public InviteSlashCommand(IMessageRepository messageRepository, 
                                  IUserRepository userRepository,
                                  IConnectionManager connectionManager)
        {
            m_MessageRepository = messageRepository;
            m_UserRepository = userRepository;
            m_ConnectionManager = connectionManager;
        }

        public string CommandText => "invite";

        public void Handle(MessageContext context, string arguments)
        {
            var parts = SplitParameters(arguments);
            if (parts.Length != 2)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: correct usage is /invite userId channelName");
                return;
            }

            var inviteeUserName = parts[0];
            var invitee = m_UserRepository.GetUserDetails(inviteeUserName);
            var channelName = parts[1];

            if (channelName.StartsWith("@"))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: channel names cannot start with '@'. This is a reserved character for direct messages.");
                return;
            }

            var inviterChannels = m_MessageRepository.GetChannelMembershipsForUser(context.User.Id);
            var inviteeChannels = m_MessageRepository.GetChannelMembershipsForUser(invitee.Id);

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

            m_MessageRepository.AddChannelMembership(invitee.Id, channelName);
            var connectionIds = m_ConnectionManager.GetConnectionsForUser(invitee);
            foreach (var connectionId in connectionIds) context.MessageHub.UpdateChannelMembership(connectionId);
            context.MessageHub.PublishSystemMessage(context.ConnectionId,
                                                    $"{invitee.UserName} is now a member of {channelName}.");

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