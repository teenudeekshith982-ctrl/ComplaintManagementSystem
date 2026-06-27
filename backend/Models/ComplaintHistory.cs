using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class ComplaintHistory
{
    [Key]
    public int HistoryId { get; set; }

    public int ComplaintId { get; set; }

    public string Action { get; set; } 

    public string Details{ get; set; } 

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }


    // Navigation Property
    public Complaint? Complaint { get; set; }
}