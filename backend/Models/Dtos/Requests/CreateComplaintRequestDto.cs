using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models.Dtos;

public class CreateComplaintRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; }
    
    [Required]
    public ComplaintCategoryEnum Category{ get; set; }
    
    public List<IFormFile>? Attachments { get; set; }
    
}