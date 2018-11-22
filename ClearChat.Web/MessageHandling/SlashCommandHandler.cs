using System;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;

namespace ClearChat.Web.MessageHandling
{
    internal class SlashCommandHandler : ISlashCommandHandler
    {
        private readonly IDictionary<string, ISlashCommand> m_Commands;

        public SlashCommandHandler(ISlashCommand[] commands)
        {
            m_Commands = commands.ToDictionary(c => c.CommandText.ToLowerInvariant(), c => c);
            var helpCommand = new HelpCommand(commands);
            m_Commands.Add(helpCommand.CommandText, helpCommand);
        }

        public void Handle(ChatContext context, string commandStringWithArguments)
        {
            if (!commandStringWithArguments.StartsWith("/"))
            {
                throw new ArgumentException("slash commands must begin with a slash");
            }

            var noSlash = commandStringWithArguments.Substring(1);

            var parts = noSlash.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var command = parts.First().ToLowerInvariant();
            var arguments = noSlash.Substring(command.Length).Trim();

            if(m_Commands.ContainsKey(command))
            {
                m_Commands[command].Handle(context, arguments);
            }
            else
            {
                context.MessageHub.PublishSystemMessage($"Command '{command}' not recognised.",
                                                 MessageScope.Caller);
            }
        }
    }
}