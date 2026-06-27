namespace ComplaintManagementSystem.Models;

public class Comment
{
    public int CommentId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string CommentedBy { get; set; } = string.Empty;
    public int ComplaintId { get; set; }

    public DateTime CreatedAt { get; set; }


    // Navigation Property
    public Complaint? Complaint { get; set; }
}