using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearChat.Core.Repositories.Bindings
{
    [Table("channelMembership")]
    public class ChannelMembershipBinding
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public byte[] ChannelName { get; set; }
    }
}