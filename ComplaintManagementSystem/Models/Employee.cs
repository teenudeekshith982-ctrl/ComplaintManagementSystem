using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Enums;
namespace ComplaintManagementSystem.Models;

public class Employee
{   
    [Key]
    public int EmployeeId { get; set; }
    
    public int DepartmentId { get; set; }
    
    public EmployeeDesignationEnum Designation { get; set; }
    public bool IsActive { get; set; }

    public int UserId { get; set; }

    // Navigation Properties
    public Department? Department { get; set; }
    public User? User { get; set; }
    public ICollection<Complaint>? Complaints { get; set; }
}