namespace ComplaintManagementSystem.Models.Dtos;

public class ComplaintTrackingDto
{
    public string Action { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string PerformedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}