using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models.Dtos;

public class EscalationActionRequestDto
{
    [Required]
    public string Action { get; set; } = string.Empty;

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(500)]
    public string Comments { get; set; } = string.Empty;
}
