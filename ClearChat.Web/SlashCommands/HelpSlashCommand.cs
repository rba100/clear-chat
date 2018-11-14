using System.Collections.Generic;
using ClearChat.Core.Domain;
using ClearChat.Web.Messaging;

namespace ClearChat.Web.SlashCommands
{
    internal class HelpSlashCommand : ISlashCommand
    {
        public string CommandText => "help";

        private readonly IReadOnlyCollection<ISlashCommand> m_Commands;

        public HelpSlashCommand(IReadOnlyCollection<ISlashCommand> commands)
        {
            m_Commands = commands;
        }

        public void Handle(User user, IMessageSink messageSink, string arguments)
        {
            messageSink.PublishSystemMessage("Available commands:", "default", MessageScope.Caller);
            foreach (var command in m_Commands)
            {
                messageSink.PublishSystemMessage($"/{command.CommandText} — {command.HelpText}", "default", MessageScope.Caller);
            }
        }

        public string HelpText => "shows available commands";
    }
}