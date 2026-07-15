using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ComplaintManagementSystem.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDepartmentRepository> _departmentRepositoryMock;
    private readonly Mock<IComplaintRepository> _complaintRepositoryMock;
    private readonly Mock<ILogger<EmployeeService>> _loggerMock;

    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _employeeRepositoryMock = new();
        _userRepositoryMock = new();
        _departmentRepositoryMock = new();
        _complaintRepositoryMock = new();
        _loggerMock = new();

        _service = new EmployeeService(
            _employeeRepositoryMock.Object,
            _loggerMock.Object,
            _userRepositoryMock.Object,
            _departmentRepositoryMock.Object,
            _complaintRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowBadRequestException_WhenEmailAlreadyExists()
    {
        var request = new CreateEmployeeRequestDto
        {
            Email = "test@test.com"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User());

        Func<Task> action =
            async () => await _service.CreateAsync(request);

        await action.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("An account with this email address already exists. Please choose a different email address.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenDepartmentNotFound()
    {
        var request = new CreateEmployeeRequestDto
        {
            Email = "test@test.com",
            Department = DepartmentEnum.Technical
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _departmentRepositoryMock
            .Setup(x => x.GetByIdAsync((int)request.Department))
            .ReturnsAsync((Department?)null);

        Func<Task> action =
            async () => await _service.CreateAsync(request);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("The specified department could not be found. Please select a valid department.");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateEmployeeSuccessfully()
    {
        var request = new CreateEmployeeRequestDto
        {
            Name = "Teenu",
            Email = "teenu@test.com",
            Phone = "9999999999",
            Password = "Password123",
            Department = DepartmentEnum.Technical,
            Designation = EmployeeDesignationEnum.Employee
        };

        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "Technical"
        };

        var user = new User
        {
            UserId = 1,
            Name = request.Name,
            Email = request.Email
        };

        var employee = new Employee
        {
            EmployeeId = 10,
            UserId = 1
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _departmentRepositoryMock
            .Setup(x => x.GetByIdAsync((int)request.Department))
            .ReturnsAsync(department);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        _employeeRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Employee>()))
            .ReturnsAsync(employee);

        var result =
            await _service.CreateAsync(request);

        result.Should().NotBeNull();

        result.EmployeeId.Should().Be(10);

        result.Name.Should().Be("Teenu");

        result.DepartmentName.Should().Be("Technical");
    }

    [Fact]
    public async Task AssignComplaintAsync_ShouldThrowNotFoundException_WhenComplaintNotFound()
    {
        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Complaint?)null);

        Func<Task> action =
            async () => await _service.AssignComplaintAsync(1);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("The requested complaint could not be found.");
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
            async () => await _service.AssignComplaintAsync(1);

        await action.Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AssignComplaintAsync_ShouldThrowBadRequestException_WhenAlreadyAssigned()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            PriorityId = 1,
            EmployeeId = 5
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        Func<Task> action =
            async () => await _service.AssignComplaintAsync(1);

        await action.Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AssignComplaintAsync_ShouldThrowNotFoundException_WhenNoEmployeesFound()
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
            async () => await _service.AssignComplaintAsync(1);

        await action.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("No active employees are currently available in the selected category for assignment.");
    }

    [Fact]
    public async Task AssignComplaintAsync_ShouldAssignComplaint_WhenSingleEmployeeExists()
    {
        var complaint = new Complaint
        {
            ComplaintId = 1,
            PriorityId = 1,
            CategoryId = 1
        };

        var employee = new Employee
        {
            EmployeeId = 10,
            User = new User
            {
                Name = "John"
            }
        };

        _complaintRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(complaint);

        _employeeRepositoryMock
            .Setup(x => x.GetLeastLoadedEmployeesAsync(1))
            .ReturnsAsync(new List<Employee>
            {
                employee
            });

        var result =
            await _service.AssignComplaintAsync(1);

        result.IsAssigned.Should().BeTrue();

        complaint.EmployeeId.Should().Be(10);

        _complaintRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Complaint>()),
            Times.Once);
    }
}