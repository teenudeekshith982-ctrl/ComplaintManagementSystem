using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ComplaintManagementSystem.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<DashboardService>> _loggerMock;
    private readonly Mock<IComplaintRepository> _complaintRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IEscalationRepository> _escalationRepositoryMock;

    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _dashboardRepositoryMock = new Mock<IDashboardRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<DashboardService>>();
        _complaintRepositoryMock = new Mock<IComplaintRepository>();
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _escalationRepositoryMock = new Mock<IEscalationRepository>();

        _service = new DashboardService(
            _dashboardRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object,
            _complaintRepositoryMock.Object,
            _employeeRepositoryMock.Object,
            _escalationRepositoryMock.Object);
    }

    [Fact]
    public async Task GetUserDashboardAsync_ShouldReturnUserDashboard()
    {
        // Arrange

        var dashboard = new UserDashboardDto();

        _currentUserServiceMock
            .Setup(x => x.GetUserId())
            .Returns(1);

        _dashboardRepositoryMock
            .Setup(x => x.GetUserDashboardAsync(1))
            .ReturnsAsync(dashboard);

        // Act

        var result = await _service.GetUserDashboardAsync();

        // Assert

        result.Should().NotBeNull();

        _dashboardRepositoryMock.Verify(
            x => x.GetUserDashboardAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task GetAdminDashboardAsync_ShouldReturnDashboardData()
    {
        // Arrange

        _complaintRepositoryMock
            .Setup(x => x.GetCountAsync())
            .ReturnsAsync(100);

        _complaintRepositoryMock
            .Setup(x => x.GetCountByStatusAsync((int)ComplaintStatusEnum.Open))
            .ReturnsAsync(20);

        _complaintRepositoryMock
            .Setup(x => x.GetCountByStatusAsync((int)ComplaintStatusEnum.InProgress))
            .ReturnsAsync(15);

        _complaintRepositoryMock
            .Setup(x => x.GetCountByStatusAsync((int)ComplaintStatusEnum.Resolved))
            .ReturnsAsync(50);

        _complaintRepositoryMock
            .Setup(x => x.GetCountByStatusAsync((int)ComplaintStatusEnum.Closed))
            .ReturnsAsync(10);

        _employeeRepositoryMock
            .Setup(x => x.GetEmployeeCountAsync())
            .ReturnsAsync(25);

        _escalationRepositoryMock
            .Setup(x => x.GetEscalationCountAsync())
            .ReturnsAsync(5);

        // Act

        var result = await _service.GetAdminDashboardAsync();

        // Assert

        result.Should().NotBeNull();

        result.TotalComplaints.Should().Be(100);

        result.OpenComplaints.Should().Be(20);

        result.InProgressComplaints.Should().Be(15);

        result.ResolvedComplaints.Should().Be(50);

        result.ClosedComplaints.Should().Be(10);

        result.TotalEmployees.Should().Be(25);

        result.EscalatedComplaints.Should().Be(5);
    }

    [Fact]
    public async Task GetAdminDashboardAsync_ShouldReturnZeroCounts_WhenNoDataExists()
    {
        // Arrange

        _complaintRepositoryMock
            .Setup(x => x.GetCountAsync())
            .ReturnsAsync(0);

        _complaintRepositoryMock
            .Setup(x => x.GetCountByStatusAsync(It.IsAny<int>()))
            .ReturnsAsync(0);

        _employeeRepositoryMock
            .Setup(x => x.GetEmployeeCountAsync())
            .ReturnsAsync(0);

        _escalationRepositoryMock
            .Setup(x => x.GetEscalationCountAsync())
            .ReturnsAsync(0);

        // Act

        var result = await _service.GetAdminDashboardAsync();

        // Assert

        result.TotalComplaints.Should().Be(0);

        result.OpenComplaints.Should().Be(0);

        result.InProgressComplaints.Should().Be(0);

        result.ResolvedComplaints.Should().Be(0);

        result.ClosedComplaints.Should().Be(0);

        result.TotalEmployees.Should().Be(0);

        result.EscalatedComplaints.Should().Be(0);
    }

    [Fact]
    public async Task GetEmployeeDashboardAsync_ShouldThrowNotFoundException_WhenEmployeeIdIsNull()
    {
        // Arrange

        _currentUserServiceMock
            .Setup(x => x.GetEmployeeId())
            .Returns((int?)null);

        // Act

        Func<Task> action =
            async () => await _service.GetEmployeeDashboardAsync();

        // Assert

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Employee not found");
    }

    [Fact]
    public async Task GetEmployeeDashboardAsync_ShouldReturnEmployeeDashboard()
    {
        // Arrange

        var dashboard = new EmployeeDashboardDto
        {
            AssignedComplaints = 5,
            InProgressComplaints = 3,
            ResolvedComplaints = 10,
            EscalatedComplaints = 2,
            OverdueComplaints = 1
        };

        _currentUserServiceMock
            .Setup(x => x.GetEmployeeId())
            .Returns(1);

        _complaintRepositoryMock
            .Setup(x => x.GetEmployeeDashboardAsync(1))
            .ReturnsAsync(dashboard);

        // Act

        var result =
            await _service.GetEmployeeDashboardAsync();

        // Assert

        result.Should().NotBeNull();

        result.AssignedComplaints.Should().Be(5);

        result.InProgressComplaints.Should().Be(3);

        result.ResolvedComplaints.Should().Be(10);

        result.EscalatedComplaints.Should().Be(2);

        result.OverdueComplaints.Should().Be(1);
    }
}