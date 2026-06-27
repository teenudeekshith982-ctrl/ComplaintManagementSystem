using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IEmployeeService
{
    
    public Task<CreateEmployeeResponseDto>CreateAsync(CreateEmployeeRequestDto request);

    
}