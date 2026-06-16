using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Models.Dtos;

public class UpdateComplaintStatusRequestDto
{
     public ComplaintStatusEnum Status { get; set; }
}