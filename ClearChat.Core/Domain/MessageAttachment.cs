namespace ClearChat.Core.Domain
{
    public class MessageAttachment
    {
        public MessageAttachment(int id, int messageId, byte[] content, string contentType)
        {
            Id = id;
            MessageId = messageId;
            Content = content;
            ContentType = contentType;
        }

        public int Id { get; }
        public int MessageId { get; }
        public byte[] Content { get; }
        public string ContentType { get; }
    }
}
