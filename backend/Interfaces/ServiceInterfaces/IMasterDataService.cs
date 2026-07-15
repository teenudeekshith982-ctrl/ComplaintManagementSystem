using System.Collections.Generic;
using System.Threading.Tasks;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces
{
    public interface IMasterDataService
    {
        Task<IEnumerable<MasterDataCategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<MasterDataPriorityDto>> GetPrioritiesAsync();
        Task<IEnumerable<MasterDataStatusDto>> GetStatusesAsync();
        Task<IEnumerable<MasterDataDepartmentDto>> GetDepartmentsAsync();
        Task<IEnumerable<MasterDataEmployeeDto>> GetEmployeesAsync(int? departmentId);
        Task<IEnumerable<MasterDataDesignationDto>> GetDesignationsAsync();
    }
}
