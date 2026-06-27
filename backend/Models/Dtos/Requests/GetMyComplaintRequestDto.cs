using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models.Dtos;

public class GetMyComplaintRequestDto
{
    [Required]
    public int PageNumber { get; set; } = 1;
    
    [Required]
    public int PageSize { get; set; } = 10;

    public ComplaintStatusEnum? Status { get; set; }

    public ComplaintCategoryEnum? Category { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? SearchTerm { get; set; }
}