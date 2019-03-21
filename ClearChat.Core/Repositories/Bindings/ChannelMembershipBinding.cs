
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearChat.Core.Repositories.Bindings
{
    [Table("channelMembership")]
    public class ChannelMembershipBinding
    {
        public int UserId { get; set; }
        public int ChannelId { get; set; }
    }
}