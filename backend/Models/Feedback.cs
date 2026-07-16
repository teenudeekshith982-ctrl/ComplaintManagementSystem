namespace ComplaintManagementSystem.Models;

public class Feedback
{
    public int FeedbackId { get; set; }

    public int ComplaintId { get; set; }

    public int Rating { get; set; }

    public string? Comments { get; set; }

    public DateTime SubmittedAt { get; set; }

    // Navigation Property
    public Complaint? Complaint { get; set; }
}
