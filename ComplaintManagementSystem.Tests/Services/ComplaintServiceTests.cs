
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
    private readonly Mock<INotificationService> _notificationServiceMock;
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
        _notificationServiceMock = new();

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
            _notificationServiceMock.Object,
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
            .WithMessage("The specified complaint category could not be found.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflictException_WhenComplaintWithSameTitleExists()
    {
        var request = new CreateComplaintRequestDto
        {
            Title = "Internet Issue",
            Description = "Network Down",
            Category = ComplaintCategoryEnum.Technical
        };

        _categoryRepositoryMock
            .Setup(x => x.ExistsAsync((int)request.Category))
            .ReturnsAsync(true);

        _currentUserServiceMock
            .Setup(x => x.GetUserId())
            .Returns(1);

        _context.Complaints.Add(new Complaint
        {
            ComplaintId = 101,
            Title = "Internet Issue",
            Description = "Already down",
            CategoryId = (int)ComplaintCategoryEnum.Technical,
            UserId = 1,
            StatusId = (int)ComplaintStatusEnum.Open,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        Func<Task> action = () => _service.CreateAsync(request);

        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("You have recently submitted this complaint. Please wait a moment before trying to submit it again.");
    }

    [Fact]
    public async Task AssignPriorityAsync_ShouldThrowBadRequestException_WhenPriorityAlreadyAssigned()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            CreatedAt = DateTime.UtcNow,
            PriorityId = 1
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        var request = new AssignPriorityRequestDto
        {
            Priority = ComplaintPriorityEnum.High
        };

        Func<Task> action = () => _service.AssignPriorityAsync(1, request);

        await action.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Priority is already assigned to this complaint.");
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

    [Fact]
    public async Task AssignComplaintToEmployeeAsync_ShouldThrowBusinessRuleException_WhenEmployeeIsNotDesignatedAsEmployee()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            PriorityId = 1,
            CategoryId = 1,
            ComplaintCategory = new ComplaintCategory { Categoryname = "Technical" }
        };

        var employee = new Employee
        {
            EmployeeId = 10,
            IsActive = true,
            Designation = EmployeeDesignationEnum.TeamLead,
            DepartmentId = 1,
            Department = new Department { DepartmentName = "Technical" },
            User = new User { Name = "John Doe" }
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _employeeRepositoryMock
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(employee);

        Func<Task> action =
            () => _service.AssignComplaintToEmployeeAsync(1, 10);

        await action.Should()
            .ThrowAsync<BusinessRuleException>()
            .WithMessage("Complaints can only be assigned to employees with the designation 'Employee'.");
    }
}