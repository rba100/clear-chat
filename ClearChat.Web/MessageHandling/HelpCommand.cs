using System.Collections.Generic;
using ClearChat.Core;
using ClearChat.Core.Domain;

namespace ClearChat.Web.MessageHandling
{
    internal class HelpCommand : ISlashCommand
    {
        public string CommandText => "help";

        private readonly IReadOnlyCollection<ISlashCommand> m_Commands;

        public HelpCommand(IReadOnlyCollection<ISlashCommand> commands)
        {
            m_Commands = commands;
        }

        public void Handle(MessageContext context, string arguments)
        {
            context.MessageHub.PublishSystemMessage("Available commands:", MessageScope.Caller);
            foreach (var command in m_Commands)
            {
                context.MessageHub.PublishSystemMessage($"/{command.CommandText} — {command.HelpText}", MessageScope.Caller);
            }
        }

        public string HelpText => "shows available commands";
    }
}