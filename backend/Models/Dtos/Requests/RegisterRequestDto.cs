using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models.Dtos;

public class RegisterRequestDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Phone]
    public string Phone { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }
}