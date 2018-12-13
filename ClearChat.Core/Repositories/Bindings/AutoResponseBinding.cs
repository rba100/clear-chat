using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearChat.Core.Repositories.Bindings
{
    [Table("AutoResponses")]
    internal sealed class AutoResponseBinding
    {
        [Key]
        public int Id { get; set; }
        public int AuthorUserId { get; set; }
        public string Substring { get; set; }
        public string Response { get; set; }
    }
}
