using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling
{
    internal class ChatMessageHandler : IMessageHandler
    {
        private readonly IMessageRepository m_MessageRepository;

        public ChatMessageHandler(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public bool Handle(MessageContext context)
        {
            if (!m_MessageRepository.GetChannelMembershipsForUser(context.UserId).Contains(context.ChannelName))
            {

                context.MessageHub.PublishSystemMessage(context.ConnectionId,
                                                        $"Error: you are not in channel {context.ChannelName}.");
                return true;
            }
            
            var chatMessage = m_MessageRepository.WriteMessage(context.UserId,
                                                               context.ChannelName,
                                                               context.Message,
                                                               DateTime.UtcNow);
            context.MessageHub.Publish(chatMessage);

            return true;
        }
    }
}