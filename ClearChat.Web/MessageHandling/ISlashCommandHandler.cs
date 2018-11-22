using ClearChat.Core.Domain;

namespace ClearChat.Web.MessageHandling
{
    public interface ISlashCommandHandler
    {
        void Handle(ChatContext context, string commandStringWithArguments);
    }
}