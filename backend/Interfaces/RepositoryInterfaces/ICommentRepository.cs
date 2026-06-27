using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Interfaces;

public interface ICommentRepository
{
    Task<Comment> AddAsync(Comment comment);
}