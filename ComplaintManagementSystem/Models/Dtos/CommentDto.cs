namespace ComplaintManagementSystem.Models.Dtos;

public class CommentDto
{
    public int CommentId { get; set; }

    public string CommentText { get; set; } = string.Empty;

    public string CommentedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}