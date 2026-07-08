using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models.Dtos;

public class ComplaintDetailsDto
{
    public string Title {get;set;}
    public string Description {get;set;}
    public string? Priority {get;set;}
    public string Status {get;set;}
    
    public List<AttachmentDto> Attachments { get; set; } = new();

    public ICollection<CommentDto> Comments { get;set; }
    public DateTime CreatedAt {get;set;}
    public string? Assignedto {get;set;}
    public DateTime? DueDate {get;set;}
    public int CreatedByUserId { get; set; }
    public int? AssignedEmployeeId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
}