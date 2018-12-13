using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearChat.Core.Repositories.Bindings
{
    [Table("messages")]
    public class MessageBinding
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        public virtual UserBinding User { get; set; }
        public int ChannelId { get; set; }
        public byte[] Message { get; set; }
        public DateTime TimeStampUtc { get; set; }
    }
}