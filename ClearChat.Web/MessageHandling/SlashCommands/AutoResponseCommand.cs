using System;
using System.Linq;
using System.Text.RegularExpressions;

using ClearChat.Core.Domain;
using ClearChat.Core.Exceptions;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    public class AutoResponseCommand : ISlashCommand
    {
        private readonly IAutoResponseRepository m_AutoResponseRepository;

        public AutoResponseCommand(IAutoResponseRepository autoResponseRepository)
        {
            m_AutoResponseRepository = autoResponseRepository
                ?? throw new ArgumentNullException(nameof(autoResponseRepository));
        }

        public string CommandText => "autoresponse";

        public void Handle(MessageContext context, string arguments)
        {
            var parameters = SplitParameters(arguments);

            if (parameters.Length != 2)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: correct usage is /autoresponse \"ping\" \"pong\"");
                return;
            }

            var substringTrigger = parameters[0];
            var response = parameters[1];

            if (substringTrigger.Length < 4)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: that's probably really annoying, so I'm gonna have to pass on that.");
                return;
            }

            if (response.Contains(substringTrigger))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: that is silly. Think about it.");
                return;
            }

            try
            {
                m_AutoResponseRepository.AddResponse(context.UserId, substringTrigger, response);
            }
            catch (DuplicateAutoResponseException)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Error: Auto response already exists for {substringTrigger}");
                return;
            }

            context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Successfully registered auto response {response} for message {substringTrigger}");
        }

        public string HelpText => "Adds an automatic response, usage /autoresponse \"ping\" \"pong\"";

        private static string[] SplitParameters(string input)
        {
            return Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Select(m => m.Value.Trim('\"'))
                .ToArray();
        }
    }
}