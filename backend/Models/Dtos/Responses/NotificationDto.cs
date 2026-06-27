namespace ComplaintManagementSystem.Models.Dtos
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RelatedComplaintId { get; set; }
        public string? RelatedComplaintTitle { get; set; }
    }
}
