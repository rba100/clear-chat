﻿using System.Collections.Generic;
using ClearChat.Core;
using ClearChat.Core.Domain;

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

        public void Handle(ChatContext context, string arguments)
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