using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling
{
    internal class ChatMessageHandler : IMessageHandler
    {
        private readonly IChatMessageFactory m_ChatMessageFactory;
        private readonly IMessageRepository m_MessageRepository;
        private readonly IChatContext m_ChatContext;

        public ChatMessageHandler(IChatMessageFactory chatMessageFactory, 
                                  IMessageRepository messageRepository,
                                  IChatContext chatContext)
        {
            m_ChatMessageFactory = chatMessageFactory;
            m_MessageRepository = messageRepository;
            m_ChatContext = chatContext;
        }

        public bool Handle(MessageContext context)
        {
            if (!m_MessageRepository.GetChannelMembershipsForUser(context.UserId).Contains(context.ChannelName))
            {

                context.MessageHub.PublishSystemMessage(context.ConnectionId,
                                                        $"Error: you are not in channel {context.ChannelName}.");
                return true;
            }
            var chatMessage = m_ChatMessageFactory.Create(context.UserId,
                                                          context.Message, 
                                                          context.ChannelName,
                                                          DateTime.UtcNow);
            
            m_MessageRepository.WriteMessage(chatMessage);
            context.MessageHub.Publish(chatMessage);

            return true;
        }
    }
}