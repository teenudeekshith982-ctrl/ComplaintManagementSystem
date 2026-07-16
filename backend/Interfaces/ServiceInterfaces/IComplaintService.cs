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

    Task UpdateCategoryAsync(
        int complaintId,
        UpdateCategoryRequestDto request);
    
    public Task<AssignComplaintResponseDto>
        AssignComplaintAsync(int complaintId);
    
    Task<AssignComplaintResponseDto> AssignComplaintToEmployeeAsync(
        int complaintId,
        int employeeId);
    
    Task UpdateComplaintStatusAsync(
        int complaintId,
        UpdateComplaintStatusRequestDto request);
    
    Task<List<ComplaintTrackingDto>>
        GetComplaintTrackingAsync(int complaintId);

    Task<ComplaintDetailsDto> GetComplaintDetailsById(int ComplaintId);
    
    Task<(string FilePath, string FileName)> GetAttachmentAsync(int complaintId, int attachmentId);

    Task<FeedbackResponseDto> SubmitFeedbackAsync(int complaintId, FeedbackRequestDto request);
}