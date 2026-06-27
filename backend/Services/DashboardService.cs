using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Services;

public class DashboardService : IDashboardService
{   
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DashboardService> _logger;
    private readonly IComplaintRepository _complaintRepository;

    private readonly IEmployeeRepository _employeeRepository;

    private readonly IEscalationRepository _escalationRepository;
    
    
    public DashboardService(IDashboardRepository dashboardRepository,
        ICurrentUserService currentUserService,
        ILogger<DashboardService> logger,
        IComplaintRepository complaintRepository,
        IEmployeeRepository employeeRepository,
        IEscalationRepository escalationRepository
        )
    {
        _dashboardRepository = dashboardRepository;
        _currentUserService = currentUserService;
        _complaintRepository = complaintRepository;
        _employeeRepository = employeeRepository;
        _escalationRepository = escalationRepository;
        _logger = logger;
    }

    public async Task<UserDashboardDto>
        GetUserDashboardAsync()
    {
        var userId =
            _currentUserService
                .GetUserId();

        _logger.LogInformation(
            "Getting dashboard for UserId {UserId}",
            userId);

        return await _dashboardRepository
            .GetUserDashboardAsync(
                userId);
    }
    
    public async Task<AdminDashboardDto>
        GetAdminDashboardAsync()
    {
        _logger.LogInformation(
            "Fetching admin dashboard");

        return new AdminDashboardDto
        {
            TotalComplaints =
                await _complaintRepository
                    .GetCountAsync(),

            OpenComplaints =
                await _complaintRepository
                    .GetCountByStatusAsync(
                        (int)ComplaintStatusEnum.Open),

            InProgressComplaints =
                await _complaintRepository
                    .GetCountByStatusAsync(
                        (int)ComplaintStatusEnum.InProgress),

            ResolvedComplaints =
                await _complaintRepository
                    .GetCountByStatusAsync(
                        (int)ComplaintStatusEnum.Resolved),

            ClosedComplaints =
                await _complaintRepository
                    .GetCountByStatusAsync(
                        (int)ComplaintStatusEnum.Closed),

            EscalatedComplaints =
                await _escalationRepository
                    .GetEscalationCountAsync(),

            TotalEmployees =
                await _employeeRepository
                    .GetEmployeeCountAsync()
        };
    }
    
    public async Task<EmployeeDashboardDto>
        GetEmployeeDashboardAsync()
    {
        var employeeId =
            _currentUserService
                .GetEmployeeId();

        if (employeeId == null)
        {
            throw new NotFoundException("Employee not found");
        }

        _logger.LogInformation("Fetching dashboard for employee {EmployeeId}",
            employeeId);

        return await _dashboardRepository
            .GetEmployeeDashboardAsync(
                employeeId.Value);
    }
}