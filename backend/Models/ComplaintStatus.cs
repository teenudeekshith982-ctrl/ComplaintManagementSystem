using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class ComplaintStatus
{   
    [Key]
    public int StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;


    // Navigation Property
    public ICollection<Complaint>? Complaints { get; set; }
}