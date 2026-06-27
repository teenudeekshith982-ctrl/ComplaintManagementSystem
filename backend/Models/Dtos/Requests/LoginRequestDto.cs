using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models.Dtos;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}