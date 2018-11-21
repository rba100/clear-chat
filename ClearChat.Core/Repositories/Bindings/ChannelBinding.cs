using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearChat.Core.Repositories.Bindings
{
    [Table("channels")]
    public class ChannelBinding
    {

        [Key]
        public int ChannelId { get; set; }
        public byte[] ChannelNameHash { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}