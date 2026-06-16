using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ComplaintManagementSystemContext _context;
    public CommentRepository(ComplaintManagementSystemContext context)
    {
        _context = context;
    }
    public async Task<Comment> AddAsync(
        Comment comment)
    {
        await _context.Comments
            .AddAsync(comment);

        await _context.SaveChangesAsync();

        return comment;
    }
}