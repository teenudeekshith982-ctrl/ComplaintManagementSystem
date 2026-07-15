using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly ComplaintManagementSystemContext _context;

        public MasterDataService(ComplaintManagementSystemContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<MasterDataCategoryDto>> GetCategoriesAsync()
        {
            return await _context.ComplaintCategories
                .Select(c => new MasterDataCategoryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Categoryname
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MasterDataPriorityDto>> GetPrioritiesAsync()
        {
            return await _context.ComplaintPriorities
                .Include(p => p.SLA)
                .Select(p => new MasterDataPriorityDto
                {
                    PriorityId = p.PriorityId,
                    PriorityName = p.Priority,
                    SlaResolutionHours = p.SLA != null ? p.SLA.ResolutionHours : 0
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MasterDataStatusDto>> GetStatusesAsync()
        {
            return await _context.ComplaintStatuses
                .Select(s => new MasterDataStatusDto
                {
                    StatusId = s.StatusId,
                    StatusName = s.StatusName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MasterDataDepartmentDto>> GetDepartmentsAsync()
        {
            return await _context.Departments
                .Select(d => new MasterDataDepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MasterDataEmployeeDto>> GetEmployeesAsync(int? departmentId)
        {
            var query = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .Where(e => e.IsActive && e.User.IsActive && e.Designation == EmployeeDesignationEnum.Employee);

            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            return await query
                .Select(e => new MasterDataEmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.User.Name,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : string.Empty,
                    Designation = e.Designation.ToString()
                })
                .ToListAsync();
        }
    }
}
