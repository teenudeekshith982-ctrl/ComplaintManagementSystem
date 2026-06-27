using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models;

public class Complaint
{
    public int ComplaintId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public int? PriorityId { get; set; }

    public int StatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public int UserId { get; set; }

    public int? EmployeeId { get; set; }


    // Navigation Properties

    public User? User { get; set; }

    public Employee? Employee { get; set; }

    public ComplaintCategory? ComplaintCategory { get; set; }

    public ComplaintPriority? ComplaintPriority { get; set; }

    public ComplaintStatus? ComplaintStatus { get; set; }

    public ICollection<Comment>? Comments { get; set; }

    public ICollection<ComplaintAttachment>? ComplaintAttachments { get; set; }

    public ICollection<ComplaintHistory>? ComplaintHistories { get; set; }

    public ICollection<EscalatedComplaint>? EscalatedComplaints { get; set; }
}