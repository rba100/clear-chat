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
        private readonly IAutoResponseRepository m_AutoResponseRepository;

        public ChatMessageHandler(IChatMessageFactory chatMessageFactory, 
                                  IMessageRepository messageRepository,
                                  IChatContext chatContext,
                                  IAutoResponseRepository autoResponseRepository)
        {
            m_ChatMessageFactory = chatMessageFactory
                ?? throw new ArgumentNullException(nameof(chatMessageFactory));

            m_MessageRepository = messageRepository
                ?? throw new ArgumentNullException(nameof(messageRepository));

            m_ChatContext = chatContext
                ?? throw new ArgumentNullException(nameof(chatContext));

            m_AutoResponseRepository = autoResponseRepository
                ?? throw new ArgumentNullException(nameof(autoResponseRepository));
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

            var autoResponse = m_AutoResponseRepository.GetResponse(chatMessage.Message);
            if (autoResponse != null)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, autoResponse);
            }

            return true;
        }
    }
}