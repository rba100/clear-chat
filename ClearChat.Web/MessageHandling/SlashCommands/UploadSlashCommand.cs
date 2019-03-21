using System;
using System.IO;
using System.Linq;
using System.Net;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    internal class UploadSlashCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;

        private static readonly string[] s_ApprovedContentTypes = { "image/jpeg" };
        private const int c_MaxUploadSizeBytes = 5 * 1024 * 1024;

        public UploadSlashCommand(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public string CommandText => "upload";

        public void Handle(MessageContext context, string arguments)
        {
            if (!Uri.IsWellFormedUriString(arguments, UriKind.Absolute))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: not a valid URL.");
                return;
            }

            var request = WebRequest.Create(arguments);
            string contentType;
            byte[] content;

            using (var response = request.GetResponse())
            {
                if (response.ContentLength > c_MaxUploadSizeBytes)
                {
                    context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: file must be less than 5MB.");
                    return;
                }

                if (!s_ApprovedContentTypes.Contains(response.ContentType))
                {
                    context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: that mime type is not white-listed.");
                    return;
                }

                contentType = response.ContentType;
                using (var memStream = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(memStream);
                    content = memStream.ToArray();
                }
            }

            var chatMessage = m_MessageRepository.WriteMessage(context.User.Id,
                                                               context.ChannelName,
                                                               "",
                                                               context.MessageTime);

            var attachmentId = m_MessageRepository.AddAttachment(chatMessage.Id, contentType, content);

            context.MessageHub.Publish(new ChatMessage(chatMessage.Id,
                                                       chatMessage.UserName,
                                                       chatMessage.ChannelName,
                                                       chatMessage.Message,
                                                       new[] { attachmentId },
                                                       chatMessage.TimeStampUtc));
        }

        public string HelpText => "messages the channel with the contents of the supplied URL.";
    }
}