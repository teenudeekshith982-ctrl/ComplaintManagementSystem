using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Enums;
using Microsoft.OpenApi.MicrosoftExtensions;

namespace ComplaintManagementSystem.Models.Dtos;

public class AssignPriorityRequestDto
{ 
    [Required(ErrorMessage = "Priority is required")]
    public ComplaintPriorityEnum Priority { get; set; }
}