using System;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Domain;
using ClearChat.Web.Messaging;

namespace ClearChat.Web.SlashCommands
{
    internal class SlashCommandHandler : ISlashCommandHandler
    {
        private readonly IDictionary<string, ISlashCommand> m_Commands;

        public SlashCommandHandler(IEnumerable<ISlashCommand> commands)
        {
            m_Commands = commands.ToDictionary(c => c.CommandText.ToLowerInvariant(), c => c);
        }

        public void Handle(User user, IMessageSink messageSink, string commandStringWithArguments)
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
                m_Commands[command].Handle(user, messageSink, arguments);
            }
            else
            {
                messageSink.PublishSystemMessage($"Command '{command}' not recognised.", 
                                                 "default", 
                                                 MessageScope.Caller);
            }
        }
    }
}