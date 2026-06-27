using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models;

public class User
{   
    [Key]
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    
    public string Role { get; set; }
    
    public DateTime JoinedDate { get; set; }
    
    public bool IsActive { get; set; }
    


    // Navigation Property
    public Employee? Employee { get; set; }
    public ICollection<Complaint>? Complaints { get; set; }
}