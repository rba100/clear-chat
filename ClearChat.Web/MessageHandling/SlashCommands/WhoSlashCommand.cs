using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    public class WhoSlashCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;

        public WhoSlashCommand(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public string CommandText => "who";
        public void Handle(MessageContext context, string arguments)
        {
            var users = m_MessageRepository.GetUsersInChannel(arguments);

            if (users == null)
            {
                context.MessageHub.PublishSystemMessage(
                    context.ConnectionId,
                    $"Channel {arguments} was not found, or there are no members of that channel.");

                return;
            }
            context.MessageHub.PublishSystemMessage(context.ConnectionId, $"The users for this channel are: {string.Join(", ", users)}");

        }

        public string HelpText => "get list of users who have joined the channel";
    }
}
