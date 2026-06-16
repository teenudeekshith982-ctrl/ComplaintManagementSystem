using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Services;

public class CommentService : ICommentService
{   
    private readonly ILogger<CommentService> _logger;
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IComplaintRepository _complaintRepository;

    public CommentService(
        ILogger<CommentService> logger,
        ICommentRepository commentRepository,
        ICurrentUserService currentUserService,
        IComplaintRepository complaintRepository)
    {
        _commentRepository = commentRepository;
        _logger = logger;
        _currentUserService = currentUserService;
        _complaintRepository = complaintRepository;
    }
    public async Task<CommentResponseDto>
        AddCommentAsync(
            int complaintId,
            CommentRequestDto request)
    {
        var complaint =
            await _complaintRepository
                .GetByIdAsync(complaintId);

        if (complaint == null)
        {
            throw new NotFoundException(
                "Complaint not found");
        }

        var role =
            _currentUserService.GetRole();

        var userId =
            _currentUserService.GetUserId();

        var employeeId =
            _currentUserService.GetEmployeeId();

        bool canComment = false;

        if (role == "Admin")
        {
            canComment = true;
        }
        else if (complaint.UserId == userId)
        {
            canComment = true;
        }
        else if (complaint.EmployeeId == employeeId)
        {
            canComment = true;
        }

        if (!canComment)
        {
            throw new UnauthorizedAccessException(
                "You cannot comment on this complaint");
        }

        var comment = new Comment
        {
            ComplaintId = complaintId,

            Message = request.CommentText,

            CommentedBy =
                $"[{role}]{_currentUserService.GetUserName()}",

            CreatedAt =
                DateTime.UtcNow
        };

        await _commentRepository
            .AddAsync(comment);

        _logger.LogInformation(
            "Comment added to Complaint {ComplaintId} by {User}",
            complaintId,
            comment.CommentedBy);

        return new CommentResponseDto
        {
            CommentId =
                comment.CommentId,

            CommentText =
                comment.Message,

            CommentedBy =
                comment.CommentedBy,

            CreatedAt =
                comment.CreatedAt
        };
    }
}