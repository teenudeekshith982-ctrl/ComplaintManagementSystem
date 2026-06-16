using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface ISlaRepository
{
    public Task<SLA?> GetByPriorityIdAsync(int priorityid);
}