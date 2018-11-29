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
        private readonly IChatHubController m_ChatHubController;

        public ChatMessageHandler(IChatMessageFactory chatMessageFactory, 
                                  IMessageRepository messageRepository,
                                  IChatHubController chatHubController)
        {
            m_ChatMessageFactory = chatMessageFactory;
            m_MessageRepository = messageRepository;
            m_ChatHubController = chatHubController;
        }

        public bool Handle(MessageContext context)
        {
            if (!m_MessageRepository.GetChannelMembershipsForUser(context.User.UserId).Contains(context.ChannelName))
            {

                context.MessageHub.PublishSystemMessage($"Error: you are not in channel {context.ChannelName}.",
                                                        MessageScope.Caller);
                return true;
            }
            var chatMessage = m_ChatMessageFactory.Create(context.User.UserId,
                                                          context.Message, 
                                                          context.ChannelName,
                                                          DateTime.UtcNow);
            
            m_MessageRepository.WriteMessage(chatMessage);
            context.MessageHub.Publish(chatMessage);

            return true;
        }
    }
}