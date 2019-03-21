using System;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling
{
    internal class ChannelPermissionHandler : IMessageHandler
    {
        public ChannelPermissionHandler()
        {
        }

        public bool Handle(MessageContext context)
        {
            if (context.ChannelName == "default" && !context.User.VerifiedPublicIdentity)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId,
                                                        "This channel is not for anonymous use. See `/help` for details on joining channels.");
                return true;
            }
            return false;
        }
    }
}