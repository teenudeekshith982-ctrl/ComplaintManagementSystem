using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class EscalatedComplaint
{   
    [Key]
    public int EscalatedId { get; set; }

    public int ComplaintId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int EscalatedLevelId { get; set; }
    
    public DateTime EscalatedAt { get; set; }


    // Navigation Properties

    public Complaint? Complaint { get; set; }

    public EscalatedLevel? EscalatedLevel { get; set; }
}