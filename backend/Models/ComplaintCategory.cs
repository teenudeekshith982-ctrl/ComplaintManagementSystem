using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class ComplaintCategory
{
    [Key]
    public int CategoryId{ get; set; }
    public string Categoryname { get; set; }
    
    //Navigation Property
    public ICollection<Complaint> Complaints { get; set; }
    
}