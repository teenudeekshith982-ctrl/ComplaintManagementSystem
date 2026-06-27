using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ComplaintManagementSystem.Tests.Services;

public class CommentServiceTests
{
    private readonly Mock<ILogger<CommentService>> _loggerMock;
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IComplaintRepository> _complaintRepositoryMock;
    private readonly ComplaintManagementSystemContext _context;

    private readonly CommentService _service;

    public CommentServiceTests()
    {
        _loggerMock = new Mock<ILogger<CommentService>>();
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _complaintRepositoryMock = new Mock<IComplaintRepository>();

        var options = new DbContextOptionsBuilder<ComplaintManagementSystemContext>()
            .UseInMemoryDatabase(databaseName: $"CommentTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ComplaintManagementSystemContext(options);

        _service = new CommentService(
            _loggerMock.Object,
            _commentRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _complaintRepositoryMock.Object,
            _context);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrowNotFoundException_WhenComplaintNotFound()
    {
        // Arrange

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Complaint?)null);

        var request = new CommentRequestDto
        {
            CommentText = "Test Comment"
        };

        // Act

        Func<Task> act =
            async () => await _service.AddCommentAsync(1, request);

        // Assert

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Complaint not found");
    }

    [Fact]
    public async Task AddCommentAsync_ShouldThrowUnauthorizedAccessException_WhenUserCannotComment()
    {
        // Arrange

        var complaint = new Complaint
        {
            ComplaintId = 1,
            UserId = 10,
            EmployeeId = 20
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _currentUserServiceMock
            .Setup(x => x.GetRole())
            .Returns("User");

        _currentUserServiceMock
            .Setup(x => x.GetUserId())
            .Returns(99);

        _currentUserServiceMock
            .Setup(x => x.GetEmployeeId())
            .Returns(100);

        var request = new CommentRequestDto
        {
            CommentText = "Test Comment"
        };

        // Act

        Func<Task> act =
            async () => await _service.AddCommentAsync(1, request);

        // Assert

        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You cannot comment on this complaint");
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAllowAdminToComment()
    {
        // Arrange

        var complaint = new Complaint
        {
            ComplaintId = 1,
            UserId = 10,
            EmployeeId = 20
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _currentUserServiceMock
            .Setup(x => x.GetRole())
            .Returns("Admin");

        _currentUserServiceMock
            .Setup(x => x.GetUserName())
            .Returns("SystemAdmin");

        var request = new CommentRequestDto
        {
            CommentText = "Admin Comment"
        };

        // Act

        var result =
            await _service.AddCommentAsync(1, request);

        // Assert

        result.Should().NotBeNull();

        result.CommentText.Should().Be("Admin Comment");

        result.CommentedBy.Should().Be("[Admin]SystemAdmin");

        _commentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Comment>()),
            Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAllowComplaintOwnerToComment()
    {
        // Arrange

        var complaint = new Complaint
        {
            ComplaintId = 1,
            UserId = 10
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _currentUserServiceMock
            .Setup(x => x.GetRole())
            .Returns("User");

        _currentUserServiceMock
            .Setup(x => x.GetUserId())
            .Returns(10);

        _currentUserServiceMock
            .Setup(x => x.GetUserName())
            .Returns("Teenu");

        var request = new CommentRequestDto
        {
            CommentText = "User Comment"
        };

        // Act

        var result =
            await _service.AddCommentAsync(1, request);

        // Assert

        result.Should().NotBeNull();

        result.CommentedBy.Should().Be("[User]Teenu");

        _commentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Comment>()),
            Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAllowAssignedEmployeeToComment()
    {
        // Arrange

        var complaint = new Complaint
        {
            ComplaintId = 1,
            UserId = 10,
            EmployeeId = 50
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _currentUserServiceMock
            .Setup(x => x.GetRole())
            .Returns("Employee");

        _currentUserServiceMock
            .Setup(x => x.GetEmployeeId())
            .Returns(50);

        _currentUserServiceMock
            .Setup(x => x.GetUserName())
            .Returns("John");

        var request = new CommentRequestDto
        {
            CommentText = "Employee Comment"
        };

        // Act

        var result =
            await _service.AddCommentAsync(1, request);

        // Assert

        result.Should().NotBeNull();

        result.CommentedBy.Should().Be("[Employee]John");

        _commentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Comment>()),
            Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldReturnCommentResponse()
    {
        // Arrange

        var complaint = new Complaint
        {
            ComplaintId = 1,
            UserId = 10
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _currentUserServiceMock
            .Setup(x => x.GetRole())
            .Returns("User");

        _currentUserServiceMock
            .Setup(x => x.GetUserId())
            .Returns(10);

        _currentUserServiceMock
            .Setup(x => x.GetUserName())
            .Returns("Teenu");

        var request = new CommentRequestDto
        {
            CommentText = "Testing Comment"
        };

        // Act

        var result =
            await _service.AddCommentAsync(1, request);

        // Assert

        result.CommentText.Should().Be("Testing Comment");

        result.CommentedBy.Should().Be("[User]Teenu");

        result.CreatedAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(5));
    }
}