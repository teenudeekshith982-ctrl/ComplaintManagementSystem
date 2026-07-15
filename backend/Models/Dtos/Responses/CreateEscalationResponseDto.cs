namespace ComplaintManagementSystem.Models.Dtos;

public class CreateEscalationResponseDto
{
    public int EscalationId { get; set; }

    public int ComplaintId { get; set; }

    public string EscalationLevel { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string RequestedBy { get; set; } = string.Empty;

    public string CurrentAssignee { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime EscalatedAt { get; set; }
}
