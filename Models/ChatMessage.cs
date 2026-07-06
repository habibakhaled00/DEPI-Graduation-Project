using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeighborHelp.Models
{
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }
        public int RequestId { get; set; }
        public string SenderId { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("RequestId")]
        public virtual HelpRequest? HelpRequest { get; set; }
    }
}
