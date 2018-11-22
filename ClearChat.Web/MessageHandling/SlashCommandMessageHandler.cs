﻿using System;
using System.Collections.Generic;
using System.Linq;

using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Web.MessageHandling.SlashCommands;

namespace ClearChat.Web.MessageHandling
{
    internal class SlashCommandMessageHandler : IMessageHandler
    {
        private readonly IDictionary<string, ISlashCommand> m_Commands;

        public SlashCommandMessageHandler(ISlashCommand[] commands)
        {
            m_Commands = commands.ToDictionary(c => c.CommandText.ToLowerInvariant(), c => c);
        }

        public bool Handle(MessageContext context)
        {
            if (!context.Message.StartsWith("/")) return false;

            var noSlash = context.Message.Substring(1);

            var parts = noSlash.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var command = parts.First().ToLowerInvariant();
            var arguments = noSlash.Substring(command.Length).Trim();

            if (m_Commands.ContainsKey(command))
            {
                m_Commands[command].Handle(context, arguments);
            }
            else if (command == "help")
            {
                context.MessageHub.PublishSystemMessage("Available commands:", MessageScope.Caller);
                foreach (var c in m_Commands.Values)
                {
                    context.MessageHub.PublishSystemMessage($"/{c.CommandText} — {c.HelpText}", MessageScope.Caller);
                }
            }
            else
            {
                context.MessageHub.PublishSystemMessage($"Command '{command}' not recognised.",
                                                        MessageScope.Caller);
            }

            return true;
        }
    }
}