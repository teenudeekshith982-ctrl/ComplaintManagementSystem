using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models.Dtos
{
    public class UpdateProfileRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;
    }
}
