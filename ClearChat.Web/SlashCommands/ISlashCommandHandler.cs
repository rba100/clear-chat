using ClearChat.Web.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace ClearChat.Web.SlashCommands
{
    public interface ISlashCommandHandler
    {
        void Handle(IMessageSink messageSink, string commandStringWithArguments);
    }
}