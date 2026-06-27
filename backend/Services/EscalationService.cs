using AutoMapper;
using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ComplaintManagementSystem.Services;

public class EscalationService : IEscalationService
{   
    private readonly IEscalationRepository _escalationRepository;
    private readonly IComplaintRepository _complaintRepository;
    private readonly IComplaintHistoryRepository _historyRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EscalationService> _logger;
    private readonly  IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ComplaintManagementSystemContext _context;
    public EscalationService(
        IEscalationRepository escalationRepository,
        IComplaintRepository complaintRepository,
        IComplaintHistoryRepository historyRepository,
        IMapper mapper,
        ILogger<EscalationService> logger,
        ICurrentUserService currentUserService,
        IEmployeeRepository employeeRepository,
        ComplaintManagementSystemContext context)
    {
        _escalationRepository = escalationRepository;
        _complaintRepository = complaintRepository;
        _historyRepository = historyRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _employeeRepository = employeeRepository;
        _context = context;

    }
        
        
        public async Task<CreateEscalationResponseDto>
    CreateEscalationAsync(
        CreateEscalationRequestDto request)
{   
    
    _logger.LogInformation(
        "Creating escalation for ComplaintId {ComplaintId}",
        request.ComplaintId);

    int? EmployeeId = _currentUserService.GetEmployeeId();
    
    

    var complaint = await _complaintRepository
        .GetByIdAsync(request.ComplaintId);

    if (complaint == null)
    {
        _logger.LogError(
            "Complaint not found. ComplaintId {ComplaintId}",
            request.ComplaintId);

        throw new NotFoundException(
            "Complaint not found");
    }

    if (complaint.EmployeeId != EmployeeId)
    {
        throw new UnauthorizedAccessException("You are not Authorised to Escalate The Complaint");
    }
    if (!Enum.IsDefined(
            typeof(EscalationLevelEnum),
            request.EscalationLevel))
    {
        _logger.LogError(
            "Invalid escalation level {EscalationLevel}",
            request.EscalationLevel);

        throw new BadRequestException("Invalid escalation level");
    }

    if (complaint.StatusId ==
        (int)ComplaintStatusEnum.Closed)
    {
        _logger.LogError(
            "Cannot escalate closed complaint. ComplaintId {ComplaintId}",
            request.ComplaintId);

        throw new BadRequestException(
            "Closed complaints cannot be escalated");
    }

    var latestEscalation =
        await _escalationRepository
            .GetLatestEscalationAsync(
                request.ComplaintId);

    EscalationLevelEnum expectedLevel;

    if (latestEscalation == null)
    {
        expectedLevel =
            EscalationLevelEnum.TeamLead;
    }
    else
    {
        expectedLevel =
            (EscalationLevelEnum)
                (latestEscalation.EscalatedLevelId + 1);
    }

    if (expectedLevel >
        EscalationLevelEnum.SeniorManager)
    {
        _logger.LogError(
            "Maximum escalation level reached for ComplaintId {ComplaintId}",
            request.ComplaintId);

        throw new BusinessRuleException("Maximum escalation level reached");
    }
    
    var escalationOwner =
        await GetEscalationOwnerAsync(
            complaint,
            expectedLevel);
    
    var oldEmployeeId =
        complaint.EmployeeId;
    
    complaint.EmployeeId =
        escalationOwner.EmployeeId;

    await _complaintRepository
        .UpdateAsync(complaint);

    if (request.EscalationLevel != expectedLevel)
    {
        _logger.LogError(
            "Invalid escalation sequence. Expected {ExpectedLevel}, Received {ReceivedLevel}",
            expectedLevel,
            request.EscalationLevel);

        throw new BadRequestException(
            $"Next escalation level must be {expectedLevel}");
    }

    var existingSameLevel = await _context.EscalatedComplaints
        .AnyAsync(e => e.ComplaintId == request.ComplaintId
            && e.EscalatedLevelId == (int)request.EscalationLevel);

    if (existingSameLevel)
    {
        throw new ConflictException("This complaint has already been escalated at this level.");
    }

    var escalation =
        new EscalatedComplaint
        {
            ComplaintId =
                request.ComplaintId,

            Reason =
                request.Reason,

            EscalatedLevelId =
                (int)request.EscalationLevel,

            EscalatedAt =
                DateTime.UtcNow
        };

    escalation =
        await _escalationRepository
            .AddAsync(escalation);

    _logger.LogInformation(
        "Escalation created successfully. EscalationId {EscalationId}",
        escalation.EscalatedId);

    await _historyRepository
        .AddAsync(
            new ComplaintHistory
            {
                ComplaintId =
                    complaint.ComplaintId,

                Action =
                    $"Escalated to {request.EscalationLevel} {escalationOwner.User.Name}",

                Details =
                    $"Escalated to {expectedLevel} and assigned to EmployeeId: {escalationOwner.User.Name}",

                ChangedBy =
                    _currentUserService
                        .GetRole(),

                CreatedAt =
                    DateTime.UtcNow
            });

    _logger.LogInformation(
        "Complaint history added successfully for ComplaintId {ComplaintId}",
        complaint.ComplaintId);

    _logger.LogInformation(
        "CreateEscalationAsync completed successfully for ComplaintId {ComplaintId}",
        complaint.ComplaintId);

    return new CreateEscalationResponseDto
    {
        EscalationId =
            escalation.EscalatedId,

        ComplaintId =
            escalation.ComplaintId,

        EscalationLevel =
            request.EscalationLevel
                .ToString(),

        Reason =
            escalation.Reason,

        EscalatedAt =
            escalation.EscalatedAt
    };
}
        public async Task AutoEscalateComplaintsAsync()
    {
        _logger.LogInformation(
            "Auto escalation process started");
    
        var complaints =
            await _complaintRepository
                .GetSlaBreachedComplaintsAsync();
    
        if (!complaints.Any())
        {
            _logger.LogInformation(
                "No SLA breached complaints found");
    
            return;
        }
    
        foreach (var complaint in complaints)
        {
            try
            {
                var latestEscalation =
                    await _escalationRepository
                        .GetLatestEscalationAsync(
                            complaint.ComplaintId);
    
                EscalationLevelEnum nextLevel;
    
                if (latestEscalation == null)
                {
                    nextLevel =
                        EscalationLevelEnum.TeamLead;
                }
                else
                {
                    nextLevel =
                        (EscalationLevelEnum)
                        (latestEscalation
                            .EscalatedLevelId + 1);
                }
    
                if (nextLevel >
                    EscalationLevelEnum
                        .SeniorManager)
                {
                    _logger.LogWarning(
                        "Maximum escalation reached for ComplaintId {ComplaintId}",
                        complaint.ComplaintId);
    
                    continue;
                }
                
                var escalationOwner =
                    await GetEscalationOwnerAsync(
                        complaint,
                        nextLevel);
    
                var oldEmployeeId =
                    complaint.EmployeeId;
    
                complaint.EmployeeId =
                    escalationOwner.EmployeeId;

                await _complaintRepository
                    .UpdateAsync(complaint);
    
                var escalation =
                    new EscalatedComplaint
                    {
                        ComplaintId =
                            complaint.ComplaintId,
    
                        EscalatedLevelId =
                            (int)nextLevel,
    
                        Reason =
                            "Automatic escalation due to SLA breach",
    
                        EscalatedAt =
                            DateTime.UtcNow
                    };
    
                await _escalationRepository
                    .AddAsync(escalation);
    
                await _historyRepository
                    .AddAsync(
                        new ComplaintHistory
                        {
                            ComplaintId =
                                complaint.ComplaintId,
    
                            Action = $"Escalated to {nextLevel.ToString()} {escalationOwner.User.Name} ",
    
                            Details =
                                $"Auto Escalated to {nextLevel}",
    
                            ChangedBy =
                                "System",
    
                            CreatedAt =
                                DateTime.UtcNow
                        });
    
                _logger.LogInformation(
                    "Complaint {ComplaintId} auto escalated to {EscalationLevel}",
                    complaint.ComplaintId,
                    nextLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error auto escalating ComplaintId {ComplaintId}",
                    complaint.ComplaintId);
            }
        }
    
        _logger.LogInformation(
            "Auto escalation process completed");
    }
        
    private async Task<Employee>
        GetEscalationOwnerAsync(
            Complaint complaint,
            EscalationLevelEnum level)
    {
        Employee? employee = null;

        switch (level)
        {
            case EscalationLevelEnum.TeamLead:

                employee =
                    await _employeeRepository
                        .GetTeamLeadByDepartmentAsync(
                            complaint.CategoryId);

                break;

            case EscalationLevelEnum.Manager:

                employee =
                    await _employeeRepository
                        .GetManagerAsync();

                break;

            case EscalationLevelEnum.SeniorManager:

                employee =
                    await _employeeRepository
                        .GetSeniorManagerAsync();

                break;
        }

        if (employee == null)
        {
            throw new NotFoundException(
                $"No employee found for escalation level {level}");
        }

        return employee;
    }
    
    public async Task<
            PagedResponseDto<
                EscalationResponseDto>>
        GetEscalationsAsync(
            EscalationFilterDto filter)
    {
        _logger.LogInformation(
            "Getting escalated complaints");

        var (
                escalations,
                totalRecords)
            =
            await _escalationRepository
                .GetEscalationsAsync(
                    filter);

        var response =
            escalations
                .Select(e =>
                    new EscalationResponseDto
                    {
                        EscalatedId =
                            e.EscalatedId,

                        ComplaintId =
                            e.ComplaintId,

                        ComplaintTitle =
                            e.Complaint!.Title,

                        Department =
                            e.Complaint
                                .Employee!
                                .Department!
                                .DepartmentName,

                        AssignedTo =
                            e.Complaint
                                .Employee!
                                .User!
                                .Name,

                        EscalationLevel =
                            e.EscalatedLevel!
                                .LevelName,

                        Reason =
                            e.Reason,

                        EscalatedAt =
                            e.EscalatedAt
                    })
                .ToList();

        _logger.LogInformation(
            "Retrieved {Count} escalations",
            response.Count);

        return new PagedResponseDto<
            EscalationResponseDto>
        {
            Data = response,

            PageNumber =
                filter.PageNumber,

            PageSize =
                filter.PageSize,

            TotalRecords =
                totalRecords
        };
    }

    public async Task<List<EscalationResponseDto>> GetComplaintEscalationsAsync(int complaintId)
    {
        var escalations = await _context.EscalatedComplaints
            .Include(e => e.EscalatedLevel)
            .Include(e => e.Complaint)
                .ThenInclude(c => c.Employee)
                    .ThenInclude(emp => emp.User)
            .Include(e => e.Complaint)
                .ThenInclude(c => c.Employee)
                    .ThenInclude(emp => emp.Department)
            .Where(e => e.ComplaintId == complaintId)
            .OrderByDescending(e => e.EscalatedAt)
            .Select(e => new EscalationResponseDto
            {
                EscalatedId = e.EscalatedId,
                ComplaintId = e.ComplaintId,
                ComplaintTitle = e.Complaint.Title,
                Department = e.Complaint.Employee != null && e.Complaint.Employee.Department != null ? e.Complaint.Employee.Department.DepartmentName : "Unknown",
                AssignedTo = e.Complaint.Employee != null && e.Complaint.Employee.User != null ? e.Complaint.Employee.User.Name : "Unassigned",
                EscalationLevel = e.EscalatedLevel != null ? e.EscalatedLevel.LevelName : "Level " + e.EscalatedLevelId,
                Reason = e.Reason,
                EscalatedAt = e.EscalatedAt
            })
            .ToListAsync();

        return escalations;
    }

    public async Task ResolveEscalationAsync(int escalatedId, string action, string comments)
    {
        var escalation = await _context.EscalatedComplaints
            .Include(e => e.Complaint)
            .FirstOrDefaultAsync(e => e.EscalatedId == escalatedId);

        if (escalation == null)
        {
            throw new NotFoundException("Escalation not found.");
        }

        var complaint = escalation.Complaint;
        if (complaint == null)
        {
            throw new NotFoundException("Associated complaint not found.");
        }

        int userId = _currentUserService.GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        string adminOrManagerName = user?.Name ?? "Manager";

        var actionLabel = action.Equals("Accept", StringComparison.OrdinalIgnoreCase) ? "Escalation Accepted" : "Escalation Declined";
        var alreadyActioned = await _context.ComplaintHistories
            .AnyAsync(h => h.ComplaintId == complaint.ComplaintId
                && h.Action == actionLabel);

        if (alreadyActioned)
        {
            throw new ConflictException("This escalation has already been actioned.");
        }

        if (action.Equals("ReassignBack", StringComparison.OrdinalIgnoreCase))
        {
            var previousAssigneeHistory = await _context.ComplaintHistories
                .Where(h => h.ComplaintId == complaint.ComplaintId && h.Action.Contains("Assigned to employee"))
                .OrderByDescending(h => h.CreatedAt)
                .Skip(1)
                .FirstOrDefaultAsync();

            Employee? targetEmployee = null;
            if (previousAssigneeHistory != null)
            {
                var employees = await _context.Employees.Include(e => e.User).ToListAsync();
                foreach (var emp in employees)
                {
                    if (emp.User != null && previousAssigneeHistory.Action.Contains(emp.User.Name))
                    {
                        targetEmployee = emp;
                        break;
                    }
                }
            }

            if (targetEmployee == null)
            {
                var employeesInCategory = await _employeeRepository.GetLeastLoadedEmployeesAsync(complaint.CategoryId);
                if (employeesInCategory.Any())
                {
                    targetEmployee = employeesInCategory.First();
                }
            }

            if (targetEmployee != null)
            {
                complaint.EmployeeId = targetEmployee.EmployeeId;
                complaint.StatusId = (int)ComplaintStatusEnum.Assigned;
                await _complaintRepository.UpdateAsync(complaint);

                await _historyRepository.AddAsync(new ComplaintHistory
                {
                    ComplaintId = complaint.ComplaintId,
                    Action = $"Escalation Declined - Reassigned",
                    Details = $"Escalation declined by {adminOrManagerName}. Reassigned back to employee: {targetEmployee.User?.Name}. Feedback: {comments}",
                    ChangedBy = _currentUserService.GetRole(),
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                throw new BadRequestException("Could not find a valid employee to reassign back to.");
            }
        }
        else if (action.Equals("Accept", StringComparison.OrdinalIgnoreCase))
        {
            await _historyRepository.AddAsync(new ComplaintHistory
            {
                ComplaintId = complaint.ComplaintId,
                Action = "Escalation Accepted",
                Details = $"Escalation accepted by {adminOrManagerName}. Comments: {comments}",
                ChangedBy = _currentUserService.GetRole(),
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            throw new BadRequestException("Invalid escalation resolution action.");
        }
    }
}