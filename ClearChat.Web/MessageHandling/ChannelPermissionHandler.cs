using System;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling
{
    internal class ChannelPermissionHandler : IMessageHandler
    {
        private readonly IUserRepository m_UserRepository;

        public ChannelPermissionHandler(IUserRepository userRepository)
        {
            m_UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public bool Handle(MessageContext context)
        {
            if (context.ChannelName == "default"
                && !m_UserRepository.GetUserDetails(context.UserId).VerifiedPublicIdentity)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "This channel is not for anonymous use. See `/help` for details on joining channels.");
                return true;
            }
            return false;
        }
    }
}