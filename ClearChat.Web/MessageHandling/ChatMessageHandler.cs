using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;
using ClearChat.Web.MessageHandling.MessageTransformers;

namespace ClearChat.Web.MessageHandling
{
    internal class ChatMessageHandler : IMessageHandler
    {
        private readonly IMessageRepository m_MessageRepository;
        private readonly IUserRepository m_UserRepository;
        private readonly IAutoResponseRepository m_AutoResponseRepository;

        public ChatMessageHandler(IMessageRepository messageRepository,
                                  IAutoResponseRepository autoresponseRepository,
                                  IUserRepository userRepository)
        {
            m_MessageRepository = messageRepository
                ?? throw new ArgumentNullException(nameof(messageRepository));

            m_AutoResponseRepository = autoresponseRepository
                ?? throw new ArgumentNullException(nameof(autoresponseRepository));
            m_UserRepository = userRepository 
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public bool Handle(MessageContext context)
        {
            if (!m_MessageRepository.GetChannelMembershipsForUser(context.User.Id).Contains(context.Channel.Name))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId,
                                                        $"Error: you are not in channel {context.Channel.Name}.");
                return true;
            }

            var message = new ImageLinkMessageTransformer().Transform(context.Message);

            var chatMessage = m_MessageRepository.WriteMessage(context.User.Id,
                                                               context.Channel.Name,
                                                               message,
                                                               DateTime.UtcNow);
            context.MessageHub.Publish(chatMessage);

            if (message != context.Message) return true;

            var channel = m_MessageRepository.GetChannel(context.Channel.Name);
            var autoResponse = m_AutoResponseRepository.GetResponse(channel.Id, chatMessage.Message);
            if (autoResponse == null) return true;
            var botAccount = m_UserRepository.GetUserDetails("ClearBot");
            var autoReponseChangeMessage = m_MessageRepository.WriteMessage(botAccount.Id,
                                                                            context.Channel.Name,
                                                                            autoResponse,
                                                                            DateTime.UtcNow);
            context.MessageHub.Publish(autoReponseChangeMessage);

            return true;
        }
    }
}