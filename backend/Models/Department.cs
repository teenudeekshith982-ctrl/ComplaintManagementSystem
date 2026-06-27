using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class Department
{       
        [Key]
        public int  DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;
        
        // Navigation Property
        public ICollection<Employee>? Employees { get; set; }
    
}