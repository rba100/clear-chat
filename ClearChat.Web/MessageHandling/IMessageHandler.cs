using ClearChat.Core.Domain;

namespace ClearChat.Web.MessageHandling
{
    public interface IMessageHandler
    {
        bool Handle(MessageContext context);
    }
}