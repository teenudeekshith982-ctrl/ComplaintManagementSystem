namespace ComplaintManagementSystem.Models.Dtos;

public class AssignComplaintResponseDto
{
        public bool IsAssigned { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<EmployeeOptionDto>? Employees { get; set; }
    
}