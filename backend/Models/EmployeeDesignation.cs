using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models
{
    public class EmployeeDesignation
    {
        [Key]
        public int DesignationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DesignationName { get; set; }

        public int? EscalationLevel { get; set; }
    }
}
