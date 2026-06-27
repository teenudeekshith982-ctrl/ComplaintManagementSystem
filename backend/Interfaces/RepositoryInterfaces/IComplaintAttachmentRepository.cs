using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface IComplaintAttachmentRepository
{
    public Task AddRangeAsync(IEnumerable<ComplaintAttachment> attachments);
}