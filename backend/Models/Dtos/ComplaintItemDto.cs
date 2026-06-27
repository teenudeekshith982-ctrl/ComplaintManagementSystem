namespace ComplaintManagementSystem.Models.Dtos;

public class ComplaintItemDto
{
    public int ComplaintId { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}