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
        private readonly IAutoResponseRepository m_AutoResponseRepository;

        public ChatMessageHandler(IMessageRepository messageRepository,
                                  IAutoResponseRepository autoresponseRepository)
        {
            m_MessageRepository = messageRepository
                ?? throw new ArgumentNullException(nameof(messageRepository));

            m_AutoResponseRepository = autoresponseRepository
                ?? throw new ArgumentNullException(nameof(autoresponseRepository));

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

            var autoResponse = m_AutoResponseRepository.GetResponse(chatMessage.Message);
            if (autoResponse == null) return true;
            var autoReponseChangeMessage = m_MessageRepository.WriteMessage("ClearBot",
                                                                            context.ChannelName,
                                                                            autoResponse,
                                                                            DateTime.UtcNow);
            context.MessageHub.Publish(autoReponseChangeMessage);

            return true;
        }
    }
}