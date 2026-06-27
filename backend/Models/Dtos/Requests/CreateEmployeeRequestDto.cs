using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models.Dtos;

public class CreateEmployeeRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = string.Empty;
    
    [Required]
    public EmployeeDesignationEnum Designation { get; set; }
    
    [Required]
    public DepartmentEnum Department { get; set; }
}