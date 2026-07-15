using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models.Dtos;

public class CreateEscalationRequestDto
{
    [Required]
    public int ComplaintId { get; set; }

    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
