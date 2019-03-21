
using ClearChat.Core.Domain;

namespace ClearChat.Web.MessageHandling
{
    internal class ChannelPermissionHandler : IMessageHandler
    {
        public bool Handle(MessageContext context)
        {
            if (context.Channel.IsDefault && !context.User.VerifiedPublicIdentity)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId,
                                                        "This channel is not for anonymous use. See `/help` for details on joining channels.");
                return true;
            }
            return false;
        }
    }
}