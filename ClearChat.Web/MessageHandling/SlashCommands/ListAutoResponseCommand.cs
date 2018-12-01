
using System;

using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    public class ListAutoResponseCommand : ISlashCommand
    {
        private readonly IAutoResponseRepository m_AutoResponseRepository;

        public ListAutoResponseCommand(IAutoResponseRepository autoResponseRepository)
        {
            m_AutoResponseRepository = autoResponseRepository
                                       ?? throw new ArgumentNullException(nameof(autoResponseRepository));
        }

        public string CommandText => "listautoresponse";

        public void Handle(MessageContext context, string arguments)
        {
            var autoResponses = m_AutoResponseRepository.GetAll();
            foreach (var response in autoResponses)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, $"\"{response.SubstringTrigger}\" => \"{response.Response}\"");
            }

            if (autoResponses.Count == 0)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "There are none.");
            }
        }

        public string HelpText => "List all automatic responses.";
    }
}