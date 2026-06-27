using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models.Dtos;

public class GetComplaintRequestDto
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public ComplaintStatusEnum? Status { get; set; }

    public ComplaintPriorityEnum? Priority { get; set; }

    public ComplaintCategoryEnum? Category { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? SearchTerm { get; set; }
}