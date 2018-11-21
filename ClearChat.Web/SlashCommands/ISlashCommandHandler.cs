using ClearChat.Core;
using ClearChat.Core.Domain;

namespace ClearChat.Web.SlashCommands
{
    public interface ISlashCommandHandler
    {
        void Handle(User user, IMessageSink messageSink, string commandStringWithArguments);
    }
}