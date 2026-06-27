using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IComplaintRepository
{
    public Task AddAsync(Complaint complaint);
    
    public Task<(List<Complaint> Complaints, int TotalRecords)>
        GetUserComplaintsAsync(
            int userId,
            GetMyComplaintRequestDto request);
    
    Task<(List<Complaint> Complaints, int TotalRecords)>
        GetComplaintsAsync(
            GetComplaintRequestDto request,
            int? employeeId = null);
    
    Task<Complaint?> GetByIdAsync(int complaintId);

    Task UpdateAsync(Complaint complaint);
    
    Task<List<Complaint>>
        GetSlaBreachedComplaintsAsync();
    
    Task<int> GetCountAsync();

    Task<int> GetCountByStatusAsync(int statusId);
    
    
    
    
    
}