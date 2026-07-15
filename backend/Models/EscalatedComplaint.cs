using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models;

public class EscalatedComplaint
{
    [Key]
    public int EscalatedId { get; set; }

    [Required]
    public int ComplaintId { get; set; }

    [ForeignKey("ComplaintId")]
    public Complaint? Complaint { get; set; }

    [Required]
    public int EscalatedLevelId { get; set; }

    [ForeignKey("EscalatedLevelId")]
    public EscalatedLevel? EscalatedLevel { get; set; }

    public int? RequestedById { get; set; }

    [ForeignKey("RequestedById")]
    public Employee? RequestedBy { get; set; }

    public int? CurrentAssigneeId { get; set; }

    [ForeignKey("CurrentAssigneeId")]
    public Employee? CurrentAssignee { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;

    public int Status { get; set; } = (int)EscalationStatusEnum.Pending;
}