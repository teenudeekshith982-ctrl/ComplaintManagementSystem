using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models.Dtos;

public class CommentRequestDto
{
    [Required]
    [MaxLength(1000)]
    public string CommentText { get; set; } = string.Empty;
}