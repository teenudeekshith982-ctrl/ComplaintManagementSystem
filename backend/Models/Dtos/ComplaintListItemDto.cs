namespace ComplaintManagementSystem.Models.Dtos;

public class ComplaintListItemDto
{
    public int ComplaintId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public string? UserName { get; set; }

    public string? AssignedTo { get; set; }
}
