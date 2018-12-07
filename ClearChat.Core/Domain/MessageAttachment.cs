using System;
using System.Collections.Generic;
using System.Text;

namespace ClearChat.Core.Domain
{
    public enum ContentEncoding { RawBinary, Utf8Url }

    public class MessageAttachment
    {
        public MessageAttachment(int id, int messageId, ContentEncoding encoding, byte[] content, string contentType)
        {
            Id = id;
            MessageId = messageId;
            Encoding = encoding;
            Content = content;
            ContentType = contentType;
        }

        public int Id { get; }
        public int MessageId { get; }
        public ContentEncoding Encoding { get; }
        public byte[] Content { get; }
        public string ContentType { get; }
    }
}
