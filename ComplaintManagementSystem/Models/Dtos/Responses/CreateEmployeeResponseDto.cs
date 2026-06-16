namespace ComplaintManagementSystem.Models.Dtos;

public class CreateEmployeeResponseDto
{
    public int EmployeeId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;
}