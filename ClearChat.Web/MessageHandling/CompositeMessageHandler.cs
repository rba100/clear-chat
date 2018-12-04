using System;
using System.Collections.Generic;
using ClearChat.Core.Domain;

namespace ClearChat.Web.MessageHandling
{
    class CompositeMessageHandler : IMessageHandler
    {
        private readonly IReadOnlyCollection<IMessageHandler> m_InnerHandlers;

        public CompositeMessageHandler(IReadOnlyCollection<IMessageHandler> innerHandlers)
        {
            m_InnerHandlers = innerHandlers;
        }

        public bool Handle(MessageContext context)
        {
            try
            {
                foreach (var messageHandler in m_InnerHandlers)
                {
                    if (messageHandler.Handle(context)) return true;
                }
            }
            catch (Exception exception)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "BOOM: " + exception.Message);
            }

            return false;
        }
    }
}