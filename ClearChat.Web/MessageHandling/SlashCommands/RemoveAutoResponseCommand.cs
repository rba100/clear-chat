using System;
using System.Linq;
using System.Text.RegularExpressions;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    public class RemoveAutoResponseCommand : ISlashCommand
    {
        private readonly IAutoResponseRepository m_AutoResponseRepository;

        public RemoveAutoResponseCommand(IAutoResponseRepository autoResponseRepository)
        {
            m_AutoResponseRepository = autoResponseRepository
                                       ?? throw new ArgumentNullException(nameof(autoResponseRepository));
        }

        public string CommandText => "removeautoresponse";

        public void Handle(MessageContext context, string arguments)
        {
            var parameters = SplitParameters(arguments);

            if (parameters.Length != 1)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: correct usage is /removeautoresponse \"ping\"");
                return;
            }

            var parameterArray = parameters.ToArray();

            m_AutoResponseRepository.RemoveResponse(parameterArray[0]);

            context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Successfully removed auto response for '{parameterArray[0]}'.");
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