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
            foreach (var messageHandler in m_InnerHandlers)
            {
                if (messageHandler.Handle(context)) return true;
            }

            return false;
        }
    }
}