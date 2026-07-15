using AutoMapper;
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

public class EscalationServiceTests
{
    private readonly Mock<IEscalationRepository> _escalationRepositoryMock = new();
    private readonly Mock<IComplaintRepository> _complaintRepositoryMock = new();
    private readonly Mock<IComplaintHistoryRepository> _historyRepositoryMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<EscalationService>> _loggerMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly ComplaintManagementSystemContext _context;

    private readonly EscalationService _service;

    public EscalationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ComplaintManagementSystemContext>()
            .UseInMemoryDatabase(databaseName: $"EscalationTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ComplaintManagementSystemContext(options);

        _service = new EscalationService(
            _escalationRepositoryMock.Object,
            _complaintRepositoryMock.Object,
            _historyRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _employeeRepositoryMock.Object,
            _context,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task CreateEscalationAsync_ShouldThrowNotFoundException_WhenComplaintNotFound()
    {
        var request = new CreateEscalationRequestDto
        {
            ComplaintId = 1,
            Reason = "Test reason"
        };

        _currentUserServiceMock
            .Setup(x => x.GetRole())
            .Returns(RolesEnum.Employee.ToString());

        _currentUserServiceMock
            .Setup(x => x.GetEmployeeId())
            .Returns(1);

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Complaint?)null);

        Func<Task> act = () => _service.CreateEscalationAsync(request);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("The requested complaint could not be found.");
    }

    [Fact]
    public async Task CreateEscalationAsync_ShouldThrowBadRequestException_WhenComplaintClosed()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            StatusId = (int)ComplaintStatusEnum.Closed,
            EmployeeId = 1
        };

        var request = new CreateEscalationRequestDto
        {
            ComplaintId = 1,
            Reason = "Test reason"
        };

        _currentUserServiceMock
            .Setup(x => x.GetRole())
            .Returns(RolesEnum.Employee.ToString());

        _currentUserServiceMock
            .Setup(x => x.GetEmployeeId())
            .Returns(1);

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        var employee = new Employee
        {
            EmployeeId = 1,
            IsActive = true,
            DesignationId = (int)EmployeeDesignationEnum.Employee
        };

        _employeeRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(employee);

        Func<Task> act = () => _service.CreateEscalationAsync(request);

        await act.Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateEscalationAsync_ShouldThrowConflictException_WhenPendingEscalationExists()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            CategoryId = 1,
            StatusId = (int)ComplaintStatusEnum.InProgress,
            EmployeeId = 1
        };

        var request = new CreateEscalationRequestDto
        {
            ComplaintId = 1,
            Reason = "Test escalation reason"
        };

        _currentUserServiceMock
            .Setup(x => x.GetRole())
            .Returns(RolesEnum.Employee.ToString());

        _currentUserServiceMock
            .Setup(x => x.GetEmployeeId())
            .Returns(1);

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        var employee = new Employee
        {
            EmployeeId = 1,
            IsActive = true,
            DesignationId = (int)EmployeeDesignationEnum.Employee
        };

        _employeeRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(employee);

        _escalationRepositoryMock
            .Setup(x => x.HasPendingEscalationAsync(1))
            .ReturnsAsync(true);

        Func<Task> act = () => _service.CreateEscalationAsync(request);

        await act.Should()
            .ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task AutoEscalateComplaintsAsync_ShouldReturn_WhenNoComplaintsExist()
    {
        _complaintRepositoryMock
            .Setup(x => x.GetSlaBreachedComplaintsAsync())
            .ReturnsAsync(new List<Complaint>());

        await _service.AutoEscalateComplaintsAsync();

        _escalationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<EscalatedComplaint>()),
            Times.Never);
    }

    [Fact]
    public async Task GetEscalationsAsync_ShouldReturnEmptyList_WhenNoEscalationsExist()
    {
        var filter = new EscalationFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        _escalationRepositoryMock
            .Setup(x => x.GetEscalationsAsync(filter))
            .ReturnsAsync((new List<EscalatedComplaint>(), 0));

        var result =
            await _service.GetEscalationsAsync(filter);

        result.Should().NotBeNull();

        result.TotalRecords.Should().Be(0);

        result.Data.Should().BeEmpty();
    }
}
