using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Repositories;
using ComplaintManagementSystem.Enums;

namespace ComplaintManagementSystem.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IComplaintRepository _complaintRepository;
    private ILogger<EmployeeService> _logger;
    public EmployeeService(IEmployeeRepository employeeRepository,
        ILogger<EmployeeService> logger,
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository,
        IComplaintRepository complaintRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _complaintRepository =  complaintRepository;
        _logger = logger;
    }
    
    public async Task<CreateEmployeeResponseDto> CreateAsync(
        CreateEmployeeRequestDto request)
    {
        _logger.LogInformation(
            "Creating employee with email {Email}",
            request.Email);

        // Validate Email
        var existingUser =
            await _userRepository
                .GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            _logger.LogError(
                "Email {Email} already exists",
                request.Email);

            throw new BadRequestException(
                "An account with this email address already exists. Please choose a different email address.");
        }

        // Validate Phone Number
        var existingUserWithPhone = await _userRepository.GetByPhoneAsync(request.Phone);
        if (existingUserWithPhone != null)
        {
            _logger.LogError(
                "Phone number {Phone} already exists",
                request.Phone);

            throw new ConflictException("An account with this phone number already exists. Please check the number and try again.");
        }

        // Validate Department
        var department =
            await _departmentRepository
                .GetByIdAsync((int)request.Department);

        if (department == null)
        {
            _logger.LogError(
                "Department {DepartmentId} not found",
                (int)request.Department);

            throw new NotFoundException(
                "The specified department could not be found. Please select a valid department.");
        }

        // Create User
        var user = new User
        {
            Name = request.Name,

            Email = request.Email,

            Phone = request.Phone,

            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password),

            JoinedDate = DateTime.UtcNow,

            IsActive = true,

            Role = RolesEnum.Employee.ToString()
        };

        user =
            await _userRepository
                .AddAsync(user);

        // Create Employee
        var employee = new Employee
        {
            UserId = user.UserId,

            DepartmentId = (int)request.Department,

            IsActive = true,
            
            Designation = request.Designation,
        };

        employee =
            await _employeeRepository
                .AddAsync(employee);

        _logger.LogInformation(
            "Employee created successfully. EmployeeId: {EmployeeId}",
            employee.EmployeeId);

        return new CreateEmployeeResponseDto
        {
            EmployeeId = employee.EmployeeId,

            UserId = user.UserId,

            Name = user.Name,

            Email = user.Email,

            DepartmentName =
                department.DepartmentName
        };
    }
    
    public async Task<AssignComplaintResponseDto>
        AssignComplaintAsync(int complaintId)
    {
        _logger.LogInformation(
            "Assigning complaint {ComplaintId}",
            complaintId);

        var complaint =
            await _complaintRepository
                .GetByIdAsync(complaintId);

        if (complaint == null)
        {
            throw new NotFoundException(
                "The requested complaint could not be found.");
        }

        if (complaint.PriorityId == null)
        {
            throw new BadRequestException(
                "A priority level must be assigned to the complaint before an employee can be assigned.");
        }

        if (complaint.EmployeeId != null)
        {
            throw new BadRequestException(
                "This complaint has already been assigned to an employee.");
        }

        var employees =
            await _employeeRepository
                .GetLeastLoadedEmployeesAsync(complaint.CategoryId);

        if (!employees.Any())
        {
            throw new NotFoundException(
                "No active employees are currently available in the selected category for assignment.");
        }

        // Multiple least-loaded employees
        if (employees.Count > 1)
        {
            return new AssignComplaintResponseDto
            {
                IsAssigned = false,

                Message =
                    "Multiple least loaded employees found",

                Employees = employees
                    .Select(e => new EmployeeOptionDto
                    {
                        EmployeeId = e.EmployeeId,

                        EmployeeName =
                            e.User!.Name
                    })
                    .ToList()
            };
        }

        // Auto Assign
        var employee = employees.First();

        complaint.EmployeeId =
            employee.EmployeeId;

        complaint.StatusId =
            (int)ComplaintStatusEnum.Assigned;

        await _complaintRepository
            .UpdateAsync(complaint);

        _logger.LogInformation(
            "Complaint {ComplaintId} assigned to Employee {EmployeeId}",
            complaintId,
            employee.EmployeeId);

        return new AssignComplaintResponseDto
        {
            IsAssigned = true,

            Message =
                $"Complaint assigned to {employee.User!.Name}"
        };
    }
}