
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearChat.Core.Repositories.Bindings
{
    [Table("MessageAttachments")]
    public class MessageAttachmentBinding
    {
        [Key]
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int Encoding { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
