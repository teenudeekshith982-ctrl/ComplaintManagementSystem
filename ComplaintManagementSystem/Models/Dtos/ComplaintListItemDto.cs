namespace ComplaintManagementSystem.Models.Dtos;

public class ComplaintListItemDto
{
    public int ComplaintId { get; set; }

    public string Title { get; set; }

    public string Status { get; set; }

    public string Priority { get; set; }

    public string Category { get; set; }

    public DateTime CreatedAt { get; set; }
}