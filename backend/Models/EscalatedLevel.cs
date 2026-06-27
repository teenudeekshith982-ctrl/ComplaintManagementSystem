using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class EscalatedLevel
{   
    [Key]
    public int EscalatedLevelId { get; set; }

    public string LevelName { get; set; } = string.Empty;


    // Navigation Property
    public ICollection<EscalatedComplaint>? EscalatedComplaints { get; set; }
}