using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;
using ClearChat.Core.Utility;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    class PurgeChannelCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;
        private readonly IUserRepository m_UserRepository;
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IStringHasher m_StringHasher;

        public PurgeChannelCommand(IMessageRepository messageRepository, 
                                   IConnectionManager connectionManager,
                                   IStringHasher stringHasher,
                                   IUserRepository userRepository)
        {
            m_MessageRepository = messageRepository;
            m_ConnectionManager = connectionManager;
            m_StringHasher = stringHasher;
            m_UserRepository = userRepository;
        }

        public string CommandText => "purge";

        public void Handle(MessageContext context, string arguments)
        {
            var userId = context.User.Id;
            var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!parts.Any())
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: correct usage is /purge channelName");
                return;
            }

            var channelName = parts[0];

            var channels = m_MessageRepository.GetChannelMembershipsForUser(userId);
            if (!channels.Contains(channelName))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: you are not a member of that channel.");
                return;
            }

            m_MessageRepository.ClearChannel(channelName);
            var users = m_MessageRepository.GetChannelMembershipsForChannel(channelName).Select(m_UserRepository.GetUserDetails);
            var connections = users.SelectMany(u => m_ConnectionManager.GetConnectionsForUser(u.UserName)).ToArray();
            foreach (var connection in connections)
            {
                context.MessageHub.SendChannelHistory(connection, channelName);
            }
            if(channelName == "default") context.MessageHub.SendChannelHistory(channelName);
        }

        public string HelpText => "Destroy all messages in a channel with: /purge channelName";
    }
}