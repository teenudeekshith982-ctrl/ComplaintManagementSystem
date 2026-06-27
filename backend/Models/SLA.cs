using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class SLA
{
    [Key]
    public int SlaId { get; set; }

    public int PriorityId { get; set; }

    public int ResolutionHours { get; set; }


    // Navigation Property
    public ComplaintPriority? ComplaintPriority { get; set; }
}