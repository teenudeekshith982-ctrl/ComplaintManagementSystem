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
            EscalationLevel = EscalationLevelEnum.TeamLead
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Complaint?)null);

        Func<Task> act = () => _service.CreateEscalationAsync(request);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Complaint not found");
    }

    [Fact]
    public async Task CreateEscalationAsync_ShouldThrowBadRequestException_WhenComplaintClosed()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            StatusId = (int)ComplaintStatusEnum.Closed
        };

        var request = new CreateEscalationRequestDto
        {
            ComplaintId = 1,
            EscalationLevel = EscalationLevelEnum.TeamLead
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        Func<Task> act = () => _service.CreateEscalationAsync(request);

        await act.Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateEscalationAsync_ShouldThrowNotFoundException_WhenTeamLeadNotFound()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            CategoryId = 1,
            StatusId = (int)ComplaintStatusEnum.InProgress
        };

        var request = new CreateEscalationRequestDto
        {
            ComplaintId = 1,
            EscalationLevel = EscalationLevelEnum.TeamLead
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _escalationRepositoryMock
            .Setup(x => x.GetLatestEscalationAsync(1))
            .ReturnsAsync((EscalatedComplaint?)null);

        _employeeRepositoryMock
            .Setup(x => x.GetTeamLeadByDepartmentAsync(1))
            .ReturnsAsync((Employee?)null);

        Func<Task> act = () => _service.CreateEscalationAsync(request);

        await act.Should()
            .ThrowAsync<NotFoundException>();
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