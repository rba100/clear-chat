using System;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling
{
    internal class ChatMessageHandler : IMessageHandler
    {
        private readonly IChatMessageFactory m_ChatMessageFactory;
        private readonly IMessageRepository m_MessageRepository;

        public ChatMessageHandler(IChatMessageFactory chatMessageFactory, 
                                  IMessageRepository messageRepository)
        {
            m_ChatMessageFactory = chatMessageFactory;
            m_MessageRepository = messageRepository;
        }

        public bool Handle(MessageContext context)
        {
            var chatMessage = m_ChatMessageFactory.Create(context.User.UserId,
                                                          context.Message, 
                                                          context.CurrentChannel,
                                                          DateTime.UtcNow);

            m_MessageRepository.WriteMessage(chatMessage);
            context.MessageHub.Publish(chatMessage);

            return true;
        }
    }
}