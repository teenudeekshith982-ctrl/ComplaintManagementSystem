namespace ComplaintManagementSystem.Models.Dtos;

public class FeedbackResponseDto
{
    public int Rating { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmittedAt { get; set; }
}
