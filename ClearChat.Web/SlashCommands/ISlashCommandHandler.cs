using ClearChat.Core.Domain;
using ClearChat.Web.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace ClearChat.Web.SlashCommands
{
    public interface ISlashCommandHandler
    {
        void Handle(User user, IMessageSink messageSink, string commandStringWithArguments);
    }
}