using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models.Dtos;

public class AssignPriorityRequestDto
{ 
    [Required(ErrorMessage = "Priority is required")]
    public ComplaintPriorityEnum Priority { get; set; }
}