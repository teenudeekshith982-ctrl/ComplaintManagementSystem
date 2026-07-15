namespace ComplaintManagementSystem.Models.Dtos
{
    public class EligibleEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class NextLevelResponseDto
    {
        public bool MaxLevelReached { get; set; }
        public string? NextLevel { get; set; }
        public int NextLevelId { get; set; }
    }
}
