
using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
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


public class ComplaintServiceTests
{
    private readonly Mock<IComplaintRepository> _complaintRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IComplaintHistoryRepository> _historyRepositoryMock;
    private readonly Mock<IComplaintAttachmentRepository> _attachmentRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<ComplaintService>> _loggerMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ISlaRepository> _slaRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IEscalationRepository> _escalationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ComplaintManagementSystemContext _context;

    private readonly ComplaintService _service;

    public ComplaintServiceTests()
    {
        _complaintRepositoryMock = new();
        _categoryRepositoryMock = new();
        _historyRepositoryMock = new();
        _attachmentRepositoryMock = new();
        _currentUserServiceMock = new();
        _loggerMock = new();
        _fileStorageServiceMock = new();
        _slaRepositoryMock = new();
        _employeeRepositoryMock = new();
        _escalationRepositoryMock = new();
        _userRepositoryMock = new();

        var options = new DbContextOptionsBuilder<ComplaintManagementSystemContext>()
            .UseInMemoryDatabase(databaseName: $"ComplaintTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ComplaintManagementSystemContext(options);

        _service = new ComplaintService(
            _complaintRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _historyRepositoryMock.Object,
            _attachmentRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object,
            _fileStorageServiceMock.Object,
            _slaRepositoryMock.Object,
            _employeeRepositoryMock.Object,
            _escalationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _context);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
    {
        var request = new CreateComplaintRequestDto
        {
            Title = "Internet Issue",
            Description = "Network Down",
            Category = ComplaintCategoryEnum.Technical
        };

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync((int)request.Category))
            .ReturnsAsync(false);

        Func<Task> action =
            () => _service.CreateAsync(request);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Category not found.");
    }

    [Fact]
    public async Task AssignPriorityAsync_ShouldThrowNotFoundException_WhenComplaintDoesNotExist()
    {
        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Complaint?)null);

        var request = new AssignPriorityRequestDto
        {
            Priority = ComplaintPriorityEnum.High
        };

        Func<Task> action =
            () => _service.AssignPriorityAsync(1, request);

        await action.Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignPriorityAsync_ShouldThrowNotFoundException_WhenSlaConfigurationDoesNotExist()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            CreatedAt = DateTime.UtcNow
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _slaRepositoryMock
            .Setup(x => x.GetByPriorityIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SLA?)null);

        var request = new AssignPriorityRequestDto
        {
            Priority = ComplaintPriorityEnum.High
        };

        Func<Task> action =
            () => _service.AssignPriorityAsync(1, request);

        await action.Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignComplaintAsync_ShouldThrowNotFoundException_WhenComplaintNotFound()
    {
        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Complaint?)null);

        Func<Task> action =
            () => _service.AssignComplaintAsync(1);

        await action.Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignComplaintAsync_ShouldThrowBadRequestException_WhenPriorityNotAssigned()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            PriorityId = null
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        Func<Task> action =
            () => _service.AssignComplaintAsync(1);

        await action.Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AssignComplaintAsync_ShouldThrowBadRequestException_WhenComplaintAlreadyAssigned()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            PriorityId = 1,
            EmployeeId = 10
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        Func<Task> action =
            () => _service.AssignComplaintAsync(1);

        await action.Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AssignComplaintAsync_ShouldThrowNotFoundException_WhenNoEmployeesExist()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            PriorityId = 1,
            CategoryId = 1
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _employeeRepositoryMock
            .Setup(x => x.GetLeastLoadedEmployeesAsync(1))
            .ReturnsAsync(new List<Employee>());

        Func<Task> action =
            () => _service.AssignComplaintAsync(1);

        await action.Should()
            .ThrowAsync<NotFoundException>();
    }
}