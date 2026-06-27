using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models.Dtos
{
    public class EscalationActionRequestDto
    {
        [Required]
        public string Action { get; set; } = string.Empty; // Accept or ReassignBack

        public string Comments { get; set; } = string.Empty;
    }
}
