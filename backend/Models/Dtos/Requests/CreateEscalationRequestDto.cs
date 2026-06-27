using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models.Dtos;

public class CreateEscalationRequestDto
{
    [Required]
    public int ComplaintId { get; set; }

    [Required]
    public EscalationLevelEnum EscalationLevel { get; set; }

    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}