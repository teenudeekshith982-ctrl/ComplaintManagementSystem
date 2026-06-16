using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface ICommentService
{
    Task<CommentResponseDto>
        AddCommentAsync(
            int complaintId,
            CommentRequestDto request);
}