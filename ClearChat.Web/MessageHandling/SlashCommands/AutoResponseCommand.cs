using System;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Domain;
using ClearChat.Core.Exceptions;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    public class AutoResponseCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;
        private readonly IAutoResponseRepository m_AutoResponseRepository;

        public AutoResponseCommand(IMessageRepository messageRepository,
                                   IAutoResponseRepository autoResponseRepository)
        {
            m_MessageRepository = messageRepository
                ?? throw new ArgumentNullException(nameof(messageRepository));

            m_AutoResponseRepository = autoResponseRepository
                ?? throw new ArgumentNullException(nameof(autoResponseRepository));
        }

        public string CommandText => "autoresponse";

        public void Handle(MessageContext context, string arguments)
        {
            var parameters = SplitParameters(arguments);

            if (parameters == null)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: correct usage is /autoresponse \"ping\" \"pong\"");
                return;
            }

            var parameterArray = parameters.ToArray();

            try
            {
                m_AutoResponseRepository.AddResponse(context.UserId, parameterArray[0], parameterArray[1]);
            }
            catch (DuplicateAutoResponseException)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Error: Auto response already exists for {parameterArray[0]}");
                return;
            }

            context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Successfully registered auto response {parameterArray[1]} for message {parameterArray[0]}");
        }

        public string HelpText => "Adds an automatic response, usage /autoresponse \"ping\" \"pong\"";

        private static IEnumerable<string> SplitParameters(string input)
        {
            var parameters = new List<string>();

            for(var i = 0; i < input.Length; i++)
            {
                if(input[i] == '"')
                {
                    var end = input.IndexOf("\"", i + 1, StringComparison.InvariantCulture);
                    var parameter = input.Substring(i, end - i).Trim('\"');
                    parameters.Add(parameter);
                    i = end;
                }
            }

            return parameters.Count != 2 ? null : parameters;
        }
    }
}