namespace ComplaintManagementSystem.Models.Dtos;

public class EscalationResponseDto
{
    public int EscalatedId { get; set; }

    public int ComplaintId { get; set; }

    public string ComplaintTitle { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public string AssignedTo { get; set; } = string.Empty;

    public int EscalatedLevelId { get; set; }

    public string EscalationLevel { get; set; } = string.Empty;

    public string RequestedBy { get; set; } = string.Empty;

    public int RequestedById { get; set; }

    public string CurrentAssignee { get; set; } = string.Empty;

    public int CurrentAssigneeId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime EscalatedAt { get; set; }

    public string Status { get; set; } = "Pending";
}
