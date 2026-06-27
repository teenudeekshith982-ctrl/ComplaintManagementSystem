using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int UserId { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? RelatedComplaintId { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public Complaint? RelatedComplaint { get; set; }
    }
}
