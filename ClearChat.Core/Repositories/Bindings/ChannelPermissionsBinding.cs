using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ClearChat.Core.Repositories.Bindings
{
    [Table("ChannelPermissions")]
    internal sealed class ChannelPermissionsBinding
    {
        [Key]
        public int Id { get; set; }
        public byte[] UserIdHash { get; set; }
        public int ChannelId { get; set; }
        public string PermissionName { get; set; }
    }
}
