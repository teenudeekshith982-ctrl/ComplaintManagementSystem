using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IComplaintService
{
    Task<CreateComplaintResponseDto> CreateAsync(
            CreateComplaintRequestDto request);

    Task<PagedComplaintResponseDto> GetMyComplaintsAsync(
        GetMyComplaintRequestDto request);
    
    Task<ComplaintListResponseDto> GetComplaintsAsync(
        GetComplaintRequestDto request);

    public Task AssignPriorityAsync(
        int complaintId,
        AssignPriorityRequestDto request);
    
    public Task<AssignComplaintResponseDto>
        AssignComplaintAsync(int complaintId);
    
    Task AssignComplaintToEmployeeAsync(
        int complaintId,
        int employeeId);
    
    Task UpdateComplaintStatusAsync(
        int complaintId,
        UpdateComplaintStatusRequestDto request);
    
    Task<List<ComplaintTrackingDto>>
        GetComplaintTrackingAsync(int complaintId);

    Task<ComplaintDetailsDto> GetComplaintDetailsById(int ComplaintId);
    
    

}