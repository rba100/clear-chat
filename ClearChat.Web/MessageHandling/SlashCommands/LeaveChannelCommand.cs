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

        public LeaveChannelCommand(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public string CommandText => "leave";

        public void Handle(MessageContext context, string arguments)
        {
            var userId = context.User.UserId;
            var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!parts.Any())
            {
                context.MessageHub.PublishSystemMessage("Error: correct usage is /leave channelName",
                                                        MessageScope.Caller);
                return;
            }

            var channelName = parts[0];

            var channels = m_MessageRepository.GetChannelMembershipsForUser(userId);
            if (!channels.Contains(channelName))
            {
                return;
            }

            m_MessageRepository.RemoveChannelMembership(userId, channelName);
            context.MessageHub.UpdateChannelMembership();
        }

        public string HelpText => "Leave a channel with: /leave channelName";
    }
}