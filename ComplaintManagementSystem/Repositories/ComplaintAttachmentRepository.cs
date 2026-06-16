using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Repositories;

public class ComplaintAttachmentRepository : IComplaintAttachmentRepository
{
    private readonly ComplaintManagementSystemContext _context;

    public ComplaintAttachmentRepository(
        ComplaintManagementSystemContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(
        IEnumerable<ComplaintAttachment> attachments)
    {
        await _context.ComplaintAttachments
            .AddRangeAsync(attachments);
    }
}