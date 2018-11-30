using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearChat.Core.Repositories.Bindings
{
    [Table("autoresponses")]
    internal sealed class AutoResponseBinding
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public string UserId { get; set; }
    }
}
