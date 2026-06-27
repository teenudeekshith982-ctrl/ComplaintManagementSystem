using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface IComplaintHistoryRepository
{
    public Task AddAsync(ComplaintHistory  complaintHistory);
    Task<List<ComplaintHistory>> GetComplaintHistoryByComplaintIdAsync(int complaintId);
}