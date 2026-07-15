namespace ComplaintManagementSystem.Models.Dtos
{
    public class MasterDataCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class MasterDataPriorityDto
    {
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public int SlaResolutionHours { get; set; }
    }

    public class MasterDataStatusDto
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    public class MasterDataDepartmentDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class MasterDataEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
    }

    public class MasterDataDesignationDto
    {
        public int DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
    }

    public class AdminCreateCategoryRequestDto
    {
        public string CategoryName { get; set; } = string.Empty;
    }

    public class AdminUpdateCategoryRequestDto
    {
        public string CategoryName { get; set; } = string.Empty;
    }

    public class AdminCreateDepartmentRequestDto
    {
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class AdminUpdateDepartmentRequestDto
    {
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class AdminCreatePriorityRequestDto
    {
        public string PriorityName { get; set; } = string.Empty;
    }

    public class AdminUpdatePriorityRequestDto
    {
        public string PriorityName { get; set; } = string.Empty;
    }

    public class AdminCreateDesignationRequestDto
    {
        public string DesignationName { get; set; } = string.Empty;
        public int? EscalationLevel { get; set; }
    }

    public class AdminUpdateDesignationRequestDto
    {
        public string DesignationName { get; set; } = string.Empty;
        public int? EscalationLevel { get; set; }
    }
}
