namespace ComplaintManagementSystem.Models.Dtos;

public class EscalationResponseDto
{
    public int EscalatedId { get; set; }

    public int ComplaintId { get; set; }

    public string ComplaintTitle { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string AssignedTo { get; set; } = string.Empty;

    public string EscalationLevel { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime EscalatedAt { get; set; }
}