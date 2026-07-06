using System.ComponentModel.DataAnnotations;

namespace NeighborHelp.ViewModels
{
    public class SendMessageViewModel
    {
        [Required]
        public string ReceiverId { get; set; } = "";

        public string ReceiverName { get; set; } = "";

        [Required, StringLength(2000)]
        public string Content { get; set; } = "";

        public int? HelpRequestId { get; set; }
    }

    public class ConversationViewModel
    {
        public string OtherUserId { get; set; } = "";
        public string OtherUserName { get; set; } = "";
        public string? OtherUserPhoto { get; set; }
        public string LastMessage { get; set; } = "";
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}
