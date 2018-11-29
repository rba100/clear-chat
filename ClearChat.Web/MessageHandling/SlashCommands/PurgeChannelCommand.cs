using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    class PurgeChannelCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IStringHasher m_StringHasher;

        public PurgeChannelCommand(IMessageRepository messageRepository, 
                                   IConnectionManager connectionManager,
                                   IStringHasher stringHasher)
        {
            m_MessageRepository = messageRepository;
            m_ConnectionManager = connectionManager;
            m_StringHasher = stringHasher;
        }

        public string CommandText => "purge";

        public void Handle(MessageContext context, string arguments)
        {
            var userId = context.User.UserId;
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
            var userIdHashes = m_MessageRepository.GetChannelMembershipsForChannel(channelName);
            var hashes = m_ConnectionManager.GetUsers().ToDictionary(u => m_StringHasher.Hash(u), u=>u);
            var affectedUserHashes = userIdHashes.Intersect(hashes.Keys).ToHashSet();
            var affectedUsers = hashes.Where(kvp => affectedUserHashes.Contains(kvp.Key));
            var affectedConnections = affectedUsers.SelectMany(u => m_ConnectionManager.GetConnectionsForUser(u.Value));
            foreach (var connection in affectedConnections)
            {
                context.MessageHub.SendChannelHistory(connection, channelName);
            }
        }

        public string HelpText => "Destroy all messages in a channel with: /purge channelName";
    }
}