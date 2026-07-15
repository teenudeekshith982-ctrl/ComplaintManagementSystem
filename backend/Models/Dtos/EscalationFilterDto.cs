namespace ComplaintManagementSystem.Models.Dtos;

public class EscalationFilterDto
{
    public int? EscalationLevelId { get; set; }

    public int? DepartmentId { get; set; }

    public string? SearchTerm { get; set; }

    public int? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}