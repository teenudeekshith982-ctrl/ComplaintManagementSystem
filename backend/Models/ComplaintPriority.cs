using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class ComplaintPriority
{
    [Key]
    public int PriorityId { get; set; }

    public string Priority { get; set; } = string.Empty;


    // Navigation Property
    public ICollection<Complaint>? Complaints { get; set; }

    public SLA? SLA { get; set; }
}