using AutoMapper;
using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Services;

public class EscalationService : IEscalationService
{
    private readonly IEscalationRepository _escalationRepository;
    private readonly IComplaintRepository _complaintRepository;
    private readonly IComplaintHistoryRepository _historyRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EscalationService> _logger;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ComplaintManagementSystemContext _context;
    private readonly INotificationService _notificationService;

    public EscalationService(
        IEscalationRepository escalationRepository,
        IComplaintRepository complaintRepository,
        IComplaintHistoryRepository historyRepository,
        IMapper mapper,
        ILogger<EscalationService> logger,
        ICurrentUserService currentUserService,
        IEmployeeRepository employeeRepository,
        ComplaintManagementSystemContext context,
        INotificationService notificationService)
    {
        _escalationRepository = escalationRepository;
        _complaintRepository = complaintRepository;
        _historyRepository = historyRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _employeeRepository = employeeRepository;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<CreateEscalationResponseDto> CreateEscalationAsync(CreateEscalationRequestDto request)
    {
        _logger.LogInformation("Creating escalation request for ComplaintId {ComplaintId}", request.ComplaintId);

        if (_currentUserService.GetRole() != RolesEnum.Employee.ToString())
        {
            throw new UnauthorizedAccessException("Only the assigned employee is permitted to request an escalation for this complaint.");
        }

        int? employeeId = _currentUserService.GetEmployeeId();

        if (!employeeId.HasValue)
        {
            int userId = _currentUserService.GetUserId();
            var employeeForUser = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == userId);
            if (employeeForUser != null)
            {
                employeeId = employeeForUser.EmployeeId;
            }
        }

        if (!employeeId.HasValue)
        {
            throw new UnauthorizedAccessException("The employee profile associated with your user account could not be found.");
        }

        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId);
        if (complaint == null)
        {
            throw new NotFoundException("The requested complaint could not be found.");
        }

        if (complaint.EmployeeId != employeeId)
        {
            throw new UnauthorizedAccessException("You do not have permission to escalate this complaint because it is not assigned to you.");
        }

        var currentEmployee = await _employeeRepository.GetByIdAsync(employeeId.Value);
        if (currentEmployee == null || !currentEmployee.IsActive)
        {
            throw new UnauthorizedAccessException("Your employee account is currently inactive. Please contact an administrator.");
        }


        if (complaint.StatusId == (int)ComplaintStatusEnum.Closed
            || complaint.StatusId == (int)ComplaintStatusEnum.Resolved)
        {
            throw new BadRequestException("Complaints that are closed or resolved cannot be escalated.");
        }

        if (complaint.StatusId != (int)ComplaintStatusEnum.InProgress
            && complaint.StatusId != (int)ComplaintStatusEnum.Reopened)
        {
            throw new BadRequestException("Only complaints that are currently in-progress or reopened can be escalated.");
        }

        bool hasPending = await _escalationRepository.HasPendingEscalationAsync(request.ComplaintId);
        if (hasPending)
        {
            throw new ConflictException("A pending escalation request already exists for this complaint. Please wait for an administrator's decision.");
        }

        var existingEscalations = await _context.EscalatedComplaints
            .Where(e => e.ComplaintId == request.ComplaintId)
            .ToListAsync();

        int nextLevelId = existingEscalations.Count + 1;
        if (nextLevelId > (int)EscalationLevelEnum.SeniorManager)
        {
            throw new BadRequestException("This complaint has already reached the maximum allowed escalation level.");
        }

        var levelName = await _escalationRepository.GetByIdAsync(nextLevelId);

        var escalation = new EscalatedComplaint
        {
            ComplaintId = request.ComplaintId,
            EscalatedLevelId = nextLevelId,
            RequestedById = employeeId.Value,
            CurrentAssigneeId = employeeId.Value,
            Reason = request.Reason,
            EscalatedAt = DateTime.UtcNow,
            Status = (int)EscalationStatusEnum.Pending
        };

        escalation = await _escalationRepository.AddAsync(escalation);

        _logger.LogInformation("Escalation request created. EscalationId {EscalationId}", escalation.EscalatedId);

        await _historyRepository.AddAsync(new ComplaintHistory
        {
            ComplaintId = complaint.ComplaintId,
            Action = "Escalation Requested",
            Details = $"Employee requested escalation to {levelName}. Reason: {request.Reason}",
            ChangedBy = _currentUserService.GetRole(),
            CreatedAt = DateTime.UtcNow
        });

        var adminUsers = await _context.Users
            .Where(u => u.Role == RolesEnum.Admin.ToString() && u.IsActive)
            .ToListAsync();

        foreach (var admin in adminUsers)
        {
            await _notificationService.CreateAsync(
                admin.UserId,
                $"New escalation request for complaint \"{complaint.Title}\". Reason: {request.Reason}",
                complaint.ComplaintId);
        }

        return new CreateEscalationResponseDto
        {
            EscalationId = escalation.EscalatedId,
            ComplaintId = escalation.ComplaintId,
            EscalationLevel = levelName ?? "Level " + nextLevelId,
            Status = "Pending",
            RequestedBy = currentEmployee.User?.Name ?? "Employee",
            CurrentAssignee = currentEmployee.User?.Name ?? "Employee",
            Reason = escalation.Reason,
            EscalatedAt = escalation.EscalatedAt
        };
    }

    public async Task AutoEscalateComplaintsAsync()
    {
        _logger.LogInformation("Auto escalation process started");

        var complaints = await _complaintRepository.GetSlaBreachedComplaintsAsync();

        if (!complaints.Any())
        {
            _logger.LogInformation("No SLA breached complaints found");
            return;
        }

        foreach (var complaint in complaints)
        {
            try
            {
                bool hasPending = await _escalationRepository.HasPendingEscalationAsync(complaint.ComplaintId);
                if (hasPending)
                {
                    _logger.LogWarning("Skipping auto-escalation for ComplaintId {ComplaintId}: pending escalation exists", complaint.ComplaintId);
                    continue;
                }

                if (complaint.StatusId == (int)ComplaintStatusEnum.Closed
                    || complaint.StatusId == (int)ComplaintStatusEnum.Resolved)
                {
                    _logger.LogWarning("Skipping auto-escalation for ComplaintId {ComplaintId}: complaint is closed/resolved", complaint.ComplaintId);
                    continue;
                }

                var existingEscalations = await _context.EscalatedComplaints
                    .Where(e => e.ComplaintId == complaint.ComplaintId)
                    .ToListAsync();

                int nextLevelId = existingEscalations.Count + 1;
                if (nextLevelId > (int)EscalationLevelEnum.SeniorManager)
                {
                    _logger.LogWarning("Maximum escalation reached for ComplaintId {ComplaintId}", complaint.ComplaintId);
                    continue;
                }

                var levelName = await _escalationRepository.GetByIdAsync(nextLevelId);
                int assigneeId = complaint.EmployeeId ?? 0;

                var escalation = new EscalatedComplaint
                {
                    ComplaintId = complaint.ComplaintId,
                    EscalatedLevelId = nextLevelId,
                    RequestedById = assigneeId,
                    CurrentAssigneeId = assigneeId,
                    Reason = "Automatic escalation due to SLA breach",
                    EscalatedAt = DateTime.UtcNow,
                    Status = (int)EscalationStatusEnum.Pending
                };

                await _escalationRepository.AddAsync(escalation);

                await _historyRepository.AddAsync(new ComplaintHistory
                {
                    ComplaintId = complaint.ComplaintId,
                    Action = "Auto-Escalation Requested",
                    Details = $"System auto-created escalation request to {levelName}. SLA breached.",
                    ChangedBy = "System",
                    CreatedAt = DateTime.UtcNow
                });

                var adminUsers = await _context.Users
                    .Where(u => u.Role == RolesEnum.Admin.ToString() && u.IsActive)
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    await _notificationService.CreateAsync(
                        admin.UserId,
                        $"Auto-escalation request for complaint \"{complaint.Title}\" due to SLA breach.",
                        complaint.ComplaintId);
                }

                _logger.LogInformation("Complaint {ComplaintId} auto-escalation request created for {EscalationLevel}",
                    complaint.ComplaintId, levelName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-escalating ComplaintId {ComplaintId}", complaint.ComplaintId);
            }
        }

        _logger.LogInformation("Auto escalation process completed");
    }

    public async Task<PagedResponseDto<EscalationResponseDto>> GetEscalationsAsync(EscalationFilterDto filter)
    {
        _logger.LogInformation("Getting escalated complaints");

        var (escalations, totalRecords) = await _escalationRepository.GetEscalationsAsync(filter);

        var response = escalations
            .Select(e => new EscalationResponseDto
            {
                EscalatedId = e.EscalatedId,
                ComplaintId = e.ComplaintId,
                ComplaintTitle = e.Complaint!.Title,
                Department = e.Complaint.Employee != null && e.Complaint.Employee.Department != null
                    ? e.Complaint.Employee.Department.DepartmentName
                    : "Unknown",
                DepartmentId = e.Complaint.Employee != null ? e.Complaint.Employee.DepartmentId : 0,
                AssignedTo = e.Complaint.Employee != null && e.Complaint.Employee.User != null
                    ? e.Complaint.Employee.User.Name
                    : "Unassigned",
                EscalatedLevelId = e.EscalatedLevelId,
                EscalationLevel = e.EscalatedLevel != null
                    ? e.EscalatedLevel.LevelName
                    : "Level " + e.EscalatedLevelId,
                RequestedBy = e.RequestedBy != null && e.RequestedBy.User != null
                    ? e.RequestedBy.User.Name
                    : "Unknown",
                RequestedById = e.RequestedById ?? 0,
                CurrentAssignee = e.CurrentAssignee != null && e.CurrentAssignee.User != null
                    ? e.CurrentAssignee.User.Name
                    : "Unknown",
                CurrentAssigneeId = e.CurrentAssigneeId ?? 0,
                Reason = e.Reason,
                EscalatedAt = e.EscalatedAt,
                Status = e.Status == (int)EscalationStatusEnum.Accepted ? "Accepted" :
                         e.Status == (int)EscalationStatusEnum.Rejected ? "Rejected" :
                         "Pending"
            })
            .ToList();

        _logger.LogInformation("Retrieved {Count} escalations", response.Count);

        return new PagedResponseDto<EscalationResponseDto>
        {
            Data = response,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<List<EscalationResponseDto>> GetComplaintEscalationsAsync(int complaintId)
    {
        var complaint = await _context.Complaints
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);

        if (complaint == null)
        {
            throw new NotFoundException("The requested complaint could not be found.");
        }

        var role = _currentUserService.GetRole();
        var canView = role == RolesEnum.Admin.ToString()
            || (role == RolesEnum.User.ToString()
                && complaint.UserId == _currentUserService.GetUserId())
            || (role == RolesEnum.Employee.ToString()
                && complaint.EmployeeId == _currentUserService.GetEmployeeId());

        if (!canView)
        {
            throw new UnauthorizedAccessException("You do not have permission to view escalation details for this complaint.");
        }

        var escalations = await _context.EscalatedComplaints
            .Include(e => e.EscalatedLevel)
            .Include(e => e.RequestedBy).ThenInclude(emp => emp.User)
            .Include(e => e.CurrentAssignee).ThenInclude(emp => emp.User)
            .Include(e => e.Complaint).ThenInclude(c => c.Employee).ThenInclude(emp => emp.User)
            .Include(e => e.Complaint).ThenInclude(c => c.Employee).ThenInclude(emp => emp.Department)
            .Where(e => e.ComplaintId == complaintId)
            .OrderByDescending(e => e.EscalatedAt)
            .Select(e => new EscalationResponseDto
            {
                EscalatedId = e.EscalatedId,
                ComplaintId = e.ComplaintId,
                ComplaintTitle = e.Complaint.Title,
                Department = e.Complaint.Employee != null && e.Complaint.Employee.Department != null
                    ? e.Complaint.Employee.Department.DepartmentName : "Unknown",
                DepartmentId = e.Complaint.Employee != null ? e.Complaint.Employee.DepartmentId : 0,
                AssignedTo = e.Complaint.Employee != null && e.Complaint.Employee.User != null
                    ? e.Complaint.Employee.User.Name : "Unassigned",
                EscalatedLevelId = e.EscalatedLevelId,
                EscalationLevel = e.EscalatedLevel != null
                    ? e.EscalatedLevel.LevelName : "Level " + e.EscalatedLevelId,
                RequestedBy = e.RequestedBy != null && e.RequestedBy.User != null
                    ? e.RequestedBy.User.Name : "Unknown",
                RequestedById = e.RequestedById ?? 0,
                CurrentAssignee = e.CurrentAssignee != null && e.CurrentAssignee.User != null
                    ? e.CurrentAssignee.User.Name : "Unknown",
                CurrentAssigneeId = e.CurrentAssigneeId ?? 0,
                Reason = e.Reason,
                EscalatedAt = e.EscalatedAt,
                Status = e.Status == (int)EscalationStatusEnum.Accepted ? "Accepted" :
                         e.Status == (int)EscalationStatusEnum.Rejected ? "Rejected" :
                         "Pending"
            })
            .ToListAsync();

        return escalations;
    }

    public async Task ResolveEscalationAsync(int escalatedId, EscalationActionRequestDto request)
    {
        var escalation = await _context.EscalatedComplaints
            .Include(e => e.Complaint)
            .Include(e => e.RequestedBy)
            .Include(e => e.CurrentAssignee)
            .FirstOrDefaultAsync(e => e.EscalatedId == escalatedId);

        if (escalation == null)
        {
            throw new NotFoundException("The requested escalation details could not be found.");
        }

        if (escalation.Status != (int)EscalationStatusEnum.Pending)
        {
            throw new ConflictException("This escalation request has already been processed and resolved.");
        }

        var complaint = escalation.Complaint;
        if (complaint == null)
        {
            throw new NotFoundException("The associated complaint for this escalation could not be found.");
        }

        int userId = _currentUserService.GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        string adminName = user?.Name ?? "Admin";

        var targetEmployee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (targetEmployee == null || !targetEmployee.IsActive)
        {
            throw new BadRequestException("The selected employee is either inactive or does not exist in the system.");
        }

        string levelName = await _escalationRepository.GetByIdAsync(escalation.EscalatedLevelId)
            ?? "Level " + escalation.EscalatedLevelId;

        if (request.Action.Equals("Accept", StringComparison.OrdinalIgnoreCase))
        {
            EmployeeDesignationEnum requiredDesignation;
            switch ((EscalationLevelEnum)escalation.EscalatedLevelId)
            {
                case EscalationLevelEnum.TeamLead:
                    requiredDesignation = EmployeeDesignationEnum.TeamLead;
                    break;
                case EscalationLevelEnum.Manager:
                    requiredDesignation = EmployeeDesignationEnum.Manager;
                    break;
                case EscalationLevelEnum.SeniorManager:
                    requiredDesignation = EmployeeDesignationEnum.SeniorManager;
                    break;
                default:
                    throw new BadRequestException("The specified escalation level is invalid.");
            }

            if (targetEmployee.DesignationId != (int)requiredDesignation)
            {
                throw new BadRequestException(
                    $"The selected employee must have the designation of {requiredDesignation} to be assigned at this escalation level.");
            }

            if (requiredDesignation == EmployeeDesignationEnum.Employee || requiredDesignation == EmployeeDesignationEnum.TeamLead)
            {
                if (targetEmployee.DepartmentId != complaint.CategoryId)
                {
                    throw new BadRequestException("The selected employee must belong to the same department as the complaint.");
                }
            }
            else
            {
                var managementDept = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Management");
                int managementDeptId = managementDept?.DepartmentId ?? 4;
                if (targetEmployee.DepartmentId != managementDeptId)
                {
                    throw new BadRequestException("The selected manager or senior manager must belong to the Management department.");
                }
            }

            escalation.Status = (int)EscalationStatusEnum.Accepted;

            complaint.EmployeeId = targetEmployee.EmployeeId;
            complaint.StatusId = (int)ComplaintStatusEnum.Assigned;

            await _context.SaveChangesAsync();

            await _historyRepository.AddAsync(new ComplaintHistory
            {
                ComplaintId = complaint.ComplaintId,
                Action = $"Escalation Accepted to {levelName}",
                Details = $"Escalation accepted by {adminName}. Complaint reassigned to {targetEmployee.User?.Name} ({requiredDesignation}). Comments: {request.Comments}",
                ChangedBy = RolesEnum.Admin.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await _notificationService.CreateAsync(
                targetEmployee.UserId,
                $"Escalation for complaint \"{complaint.Title}\" was accepted. Complaint is now assigned to you. Comments: {request.Comments}",
                complaint.ComplaintId);

            await _notificationService.CreateAsync(
                complaint.UserId,
                $"Escalation for your complaint \"{complaint.Title}\" was accepted. It has been escalated to {levelName}. Comments: {request.Comments}",
                complaint.ComplaintId);

            if (escalation.RequestedById != targetEmployee.EmployeeId)
            {
                await _notificationService.CreateAsync(
                    (await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == escalation.RequestedById))?.UserId ?? 0,
                    $"Your escalation request for complaint \"{complaint.Title}\" was accepted and escalated to {levelName}.",
                    complaint.ComplaintId);
            }
        }
        else if (request.Action.Equals("Reject", StringComparison.OrdinalIgnoreCase))
        {
            var expectedDesignationId = escalation.RequestedBy?.DesignationId ?? (int)EmployeeDesignationEnum.Employee;
            if (targetEmployee.DesignationId != expectedDesignationId)
            {
                throw new BadRequestException("Upon rejecting the escalation, the complaint must be reassigned to an active employee belonging to the same department with the same designation.");
            }

            if (expectedDesignationId == (int)EmployeeDesignationEnum.Employee || expectedDesignationId == (int)EmployeeDesignationEnum.TeamLead)
            {
                if (targetEmployee.DepartmentId != complaint.CategoryId)
                {
                    throw new BadRequestException("The selected employee must belong to the same department as the complaint.");
                }
            }
            else
            {
                var managementDept = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Management");
                int managementDeptId = managementDept?.DepartmentId ?? 4;
                if (targetEmployee.DepartmentId != managementDeptId)
                {
                    throw new BadRequestException("The selected manager or senior manager must belong to the Management department.");
                }
            }

            escalation.Status = (int)EscalationStatusEnum.Rejected;

            complaint.EmployeeId = targetEmployee.EmployeeId;
            complaint.StatusId = (int)ComplaintStatusEnum.Assigned;

            await _context.SaveChangesAsync();

            await _historyRepository.AddAsync(new ComplaintHistory
            {
                ComplaintId = complaint.ComplaintId,
                Action = "Escalation Rejected",
                Details = $"Escalation rejected by {adminName}. Complaint reassigned to {targetEmployee.User?.Name} ({targetEmployee.Designation?.DesignationName}). Comments: {request.Comments}",
                ChangedBy = RolesEnum.Admin.ToString(),
                CreatedAt = DateTime.UtcNow
            });

            await _notificationService.CreateAsync(
                targetEmployee.UserId,
                $"Escalation for complaint \"{complaint.Title}\" was rejected. Complaint is now assigned to you. Comments: {request.Comments}",
                complaint.ComplaintId);

            await _notificationService.CreateAsync(
                complaint.UserId,
                $"Escalation for your complaint \"{complaint.Title}\" was rejected. It has been reassigned. Comments: {request.Comments}",
                complaint.ComplaintId);
        }
        else
        {
            throw new BadRequestException("The specified resolution action is invalid. It must be either 'Accept' or 'Reject'.");
        }
    }

    public async Task<int> GetPendingEscalationCountAsync()
    {
        return await _context.EscalatedComplaints
            .CountAsync(e => e.Status == (int)EscalationStatusEnum.Pending);
    }

    public async Task<IEnumerable<EligibleEmployeeDto>> GetEligibleEmployeesAsync(int complaintId, string action)
    {
        var escalation = await _context.EscalatedComplaints
            .Include(e => e.Complaint)
            .Include(e => e.RequestedBy)
            .FirstOrDefaultAsync(e => e.EscalatedId == complaintId);

        if (escalation == null || escalation.Complaint == null)
        {
            throw new NotFoundException("Escalation not found.");
        }

        int departmentId = escalation.Complaint.CategoryId;

        if (action.Equals("Accept", StringComparison.OrdinalIgnoreCase))
        {
            EmployeeDesignationEnum requiredDesignation;
            switch ((EscalationLevelEnum)escalation.EscalatedLevelId)
            {
                case EscalationLevelEnum.TeamLead:
                    requiredDesignation = EmployeeDesignationEnum.TeamLead;
                    break;
                case EscalationLevelEnum.Manager:
                    requiredDesignation = EmployeeDesignationEnum.Manager;
                    break;
                case EscalationLevelEnum.SeniorManager:
                    requiredDesignation = EmployeeDesignationEnum.SeniorManager;
                    break;
                default:
                    throw new BadRequestException("Invalid escalation level.");
            }

            var query = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Where(e => e.IsActive && e.User.IsActive
                    && e.DesignationId == (int)requiredDesignation);

            if (requiredDesignation == EmployeeDesignationEnum.Employee || requiredDesignation == EmployeeDesignationEnum.TeamLead)
            {
                query = query.Where(e => e.DepartmentId == departmentId);
            }
            else
            {
                var managementDept = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Management");
                int managementDeptId = managementDept?.DepartmentId ?? 4;
                query = query.Where(e => e.DepartmentId == managementDeptId);
            }

            return await query
                .Select(e => new EligibleEmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.User.Name,
                    Designation = e.Designation != null ? e.Designation.DesignationName : "",
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : ""
                })
                .ToListAsync();
        }
        else if (action.Equals("Reject", StringComparison.OrdinalIgnoreCase))
        {
            var requiredDesignationId = escalation.RequestedBy?.DesignationId ?? (int)EmployeeDesignationEnum.Employee;

            var query = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Where(e => e.IsActive && e.User.IsActive
                    && e.DesignationId == requiredDesignationId);

            if (requiredDesignationId == (int)EmployeeDesignationEnum.Employee || requiredDesignationId == (int)EmployeeDesignationEnum.TeamLead)
            {
                query = query.Where(e => e.DepartmentId == departmentId);
            }
            else
            {
                var managementDept = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Management");
                int managementDeptId = managementDept?.DepartmentId ?? 4;
                query = query.Where(e => e.DepartmentId == managementDeptId);
            }

            return await query
                .Select(e => new EligibleEmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.User.Name,
                    Designation = e.Designation != null ? e.Designation.DesignationName : "",
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : ""
                })
                .ToListAsync();
        }

        throw new BadRequestException("Invalid action. Must be 'Accept' or 'Reject'.");
    }

    public async Task<NextLevelResponseDto> GetNextEscalationLevelAsync(int complaintId)
    {
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);

        if (complaint == null)
        {
            throw new NotFoundException("Complaint not found.");
        }

        var existingEscalations = await _context.EscalatedComplaints
            .Where(e => e.ComplaintId == complaintId)
            .ToListAsync();

        int nextLevelId = existingEscalations.Count + 1;

        if (nextLevelId > (int)EscalationLevelEnum.SeniorManager)
        {
            return new NextLevelResponseDto { MaxLevelReached = true, NextLevel = null, NextLevelId = 0 };
        }

        var levelName = await _context.EscalatedLevels
            .Where(l => l.EscalatedLevelId == nextLevelId)
            .Select(l => l.LevelName)
            .FirstOrDefaultAsync();

        return new NextLevelResponseDto { MaxLevelReached = false, NextLevel = levelName, NextLevelId = nextLevelId };
    }
}
