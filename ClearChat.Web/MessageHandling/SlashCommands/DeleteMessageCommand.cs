
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    class DeleteMessageCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;

        public DeleteMessageCommand(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public string CommandText => "delete";

        public void Handle(MessageContext context, string arguments)
        {
            if (int.TryParse(arguments.Trim(), out int messageId))
            {
                m_MessageRepository.DeleteMessage(messageId);
                context.MessageHub.PublishMessageDeleted(messageId);
            }
            else
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Could not parse '{arguments}' as a integer.");
            }
        }

        public string HelpText => "delete a chat message someone has written by its ID.";
    }
}