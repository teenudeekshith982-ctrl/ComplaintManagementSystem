using System;

namespace ComplaintManagementSystem.Models.Dtos
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public ProfileEmployeeInfoDto? EmployeeInfo { get; set; }
    }

    public class ProfileEmployeeInfoDto
    {
        public int EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }
}
