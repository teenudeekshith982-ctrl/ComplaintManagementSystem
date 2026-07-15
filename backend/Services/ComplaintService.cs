using System.ComponentModel.DataAnnotations;
using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Services;

public class ComplaintService : IComplaintService
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IComplaintHistoryRepository _historyRepository;
    private readonly IComplaintAttachmentRepository _attachmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ComplaintService> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISlaRepository _slaRepository;
    private readonly IEmployeeRepository  _employeeRepository;
    private readonly IEscalationRepository _escalationRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ComplaintManagementSystemContext _context;

    public ComplaintService(
        IComplaintRepository complaintRepository,
        ICategoryRepository categoryRepository,
        IComplaintHistoryRepository historyRepository,
        IComplaintAttachmentRepository attachmentRepository,
        ICurrentUserService currentUserService,
        ILogger<ComplaintService> logger,
        IFileStorageService fileStorageService,
        ISlaRepository  slaRepository,
        IEmployeeRepository   employeeRepository,
        IEscalationRepository escalationRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        ComplaintManagementSystemContext context)
    {
        _complaintRepository = complaintRepository;
        _categoryRepository = categoryRepository;
        _historyRepository = historyRepository;
        _attachmentRepository = attachmentRepository;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _slaRepository = slaRepository;
        _employeeRepository = employeeRepository;
        _escalationRepository = escalationRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _context = context;
        _logger = logger;
    }

    public async Task<CreateComplaintResponseDto>  CreateAsync(CreateComplaintRequestDto request)
    {
        _logger.LogInformation("Creating Complaint");
        
        bool categoryExists =
            await _categoryRepository
                .ExistsAsync((int)request.Category);

        if (!categoryExists)
        {
            throw new NotFoundException("The specified complaint category could not be found.");
        }
        
        int userId =
            _currentUserService.GetUserId();

        var recentDuplicate = await _context.Complaints
            .AnyAsync(c => c.UserId == userId
                && c.Title == request.Title
                && c.CreatedAt > DateTime.UtcNow.AddSeconds(-30));

        if (recentDuplicate)
        {
            throw new ConflictException("You have recently submitted this complaint. Please wait a moment before trying to submit it again.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        
        var complaint = new Complaint
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = (int)request.Category,

            UserId = userId,

            StatusId = (int)ComplaintStatusEnum.Open,

            PriorityId = null,

            EmployeeId = null,

            DueDate = null,

            CreatedAt = DateTime.UtcNow
        };
        
        await _complaintRepository.AddAsync(complaint);

        if (request.Attachments is not null &&
            request.Attachments.Any())
        {
            var uploadedFiles =
                await _fileStorageService
                    .UploadFilesAsync(
                        request.Attachments,
                        $"complaints/{complaint.ComplaintId}");
            var attachments =
                uploadedFiles
                    .Select(file =>
                        new ComplaintAttachment
                        {
                            ComplaintId =
                                complaint.ComplaintId,

                            FilePath =
                                file.FilePath,
                            
                            FileName = file.FileName,

                            ContentType =
                                file.ContentType,

                            BlobName =
                                file.BlobName,
                            
                            UploadedAt = DateTime.UtcNow
                        })
                    .ToList();
        
        await _attachmentRepository
            .AddRangeAsync(
                attachments);
    }
        var history = new ComplaintHistory
        {
            ComplaintId = complaint.ComplaintId,

            Action = "Complaint Created",

            Details = "Complaint Registered Successfully",

            ChangedBy = user.Name,

            CreatedAt = DateTime.UtcNow
        };
        
        await _historyRepository
            .AddAsync(history);
        
        await _notificationService.CreateAsync(
            userId,
            $"Your complaint \"{complaint.Title}\" has been submitted successfully (Status: Open).",
            complaint.ComplaintId);

        _logger.LogInformation(
            "Complaint {ComplaintId} created successfully",
            complaint.ComplaintId);
        
        return new CreateComplaintResponseDto
        {
            ComplaintId = complaint.ComplaintId,
            Title = complaint.Title,
            Status = "Open",
            CreatedAt = complaint.CreatedAt
        };
    }


    public async Task<PagedComplaintResponseDto> GetMyComplaintsAsync(
        GetMyComplaintRequestDto request)
    {
        var userId =
            _currentUserService.GetUserId();
        
        var result =
            await _complaintRepository
                .GetUserComplaintsAsync(
                    userId,
                    request);
        
        var complaints =
            result.Complaints
                .Select(c => new ComplaintItemDto()
                {
                    ComplaintId = c.ComplaintId,

                    Title = c.Title,

                    Status = ((ComplaintStatusEnum)c.StatusId).ToString(),

                    CreatedAt = c.CreatedAt
                })
                .ToList();
        
        return new PagedComplaintResponseDto
        {
            Items = complaints,

            PageNumber = request.PageNumber,

            PageSize = request.PageSize,

            TotalRecords = result.TotalRecords
        };
        
        
    }

    public async Task<ComplaintListResponseDto>
        GetComplaintsAsync(
            GetComplaintRequestDto request)
    {
        int? employeeId = null;

        var role = _currentUserService.GetRole();

        if (role == "Employee")
        {
            employeeId =
                _currentUserService.GetEmployeeId();
        }

        var (complaints, totalRecords) =
            await _complaintRepository
                .GetComplaintsAsync(
                    request,
                    employeeId);

        var response = new ComplaintListResponseDto
        {
            Complaints = complaints.Select(c =>
                new ComplaintListItemDto
                {
                    ComplaintId = c.ComplaintId,
                    Title = c.Title,
                    Status = ((ComplaintStatusEnum)c.StatusId).ToString(),
                    Priority = c.PriorityId.HasValue
                                   ? ((ComplaintPriorityEnum)c.PriorityId.Value).ToString()
                                   : "Not Assigned By Admin",
                    Category = ((ComplaintCategoryEnum)c.CategoryId).ToString(),
                    CreatedAt = c.CreatedAt
                }).ToList(),

            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return response;
    }

    public async Task AssignPriorityAsync(
        int complaintId,
        AssignPriorityRequestDto request)
    {
        _logger.LogInformation("Assigning the priority {ComplaintId} started", complaintId);
        
        if (!Enum.IsDefined(
                typeof(ComplaintPriorityEnum),
                request.Priority))
        {
            throw new BadRequestException("The selected priority level is invalid. Please select a valid option.");
        }
        
        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null)
        {
            _logger.LogError("Complaint not found this id to update {complaintId}", complaintId);
            throw new NotFoundException("The requested complaint could not be found.");
        }

        if (complaint.PriorityId.HasValue)
        {
            throw new BadRequestException("Priority is already assigned to this complaint.");
        }
        
        int priorityId = (int)request.Priority;
        complaint.PriorityId =  priorityId;
        
        var sla = await _slaRepository.GetByPriorityIdAsync(priorityId);

        if (sla == null)
        {
            _logger.LogError("Priority {PriorityId} does not exist", priorityId);
            throw new NotFoundException("The SLA configuration for the selected priority could not be found.");
        }

        complaint.DueDate = complaint.CreatedAt.AddHours(sla.ResolutionHours);
        
        
        await _complaintRepository.UpdateAsync(complaint);
        _logger.LogInformation(
            "Assigning priority {Priority} to complaint {ComplaintId}",
            request.Priority,
            complaintId);
        _logger.LogInformation("Due Date Assigned to the complaint {ComplaintId}", complaintId);
        await _historyRepository.AddAsync(
            new ComplaintHistory()
            {
                ComplaintId = complaintId,
                Action = "Priority Assigned",
                Details = $"Priority Changed to {request.Priority}",
                ChangedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            });

        await _notificationService.CreateAsync(
            complaint.UserId,
            $"Priority for your complaint \"{complaint.Title}\" has been set to {request.Priority}.",
            complaintId);
    }

    public async Task UpdateCategoryAsync(
        int complaintId,
        UpdateCategoryRequestDto request)
    {
        _logger.LogInformation("Updating category of complaint {ComplaintId} to {CategoryId}", complaintId, request.CategoryId);

        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null)
        {
            throw new NotFoundException("The requested complaint could not be found.");
        }

        bool categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
        if (!categoryExists)
        {
            throw new NotFoundException("The specified complaint category could not be found.");
        }

        int oldCategoryId = complaint.CategoryId;
        complaint.CategoryId = request.CategoryId;

        // Reset assignment if category department changes
        if (oldCategoryId != request.CategoryId)
        {
            complaint.EmployeeId = null;
            complaint.StatusId = (int)ComplaintStatusEnum.Open;
        }

        await _complaintRepository.UpdateAsync(complaint);

        await _historyRepository.AddAsync(new ComplaintHistory
        {
            ComplaintId = complaintId,
            Action = "Category Updated",
            Details = $"Category changed from {oldCategoryId} to {request.CategoryId}. Employee assignment reset.",
            ChangedBy = "Admin",
            CreatedAt = DateTime.UtcNow
        });
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
            throw new NotFoundException("The requested complaint could not be found.");
        }

        if (complaint.PriorityId == null)
        {
            throw new BadRequestException(
                "A priority level must be assigned to the complaint before an employee can be assigned.");
        }

        if (complaint.EmployeeId != null)
        {
            throw new BadRequestException("This complaint has already been assigned to an employee.");
        }

        var employees =
            await _employeeRepository
                .GetLeastLoadedEmployeesAsync(complaint.CategoryId);

        if (!employees.Any())
        {
            throw new NotFoundException("No active employees are currently available in the selected category for assignment.");
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

        string oldStatus = ((ComplaintStatusEnum)complaint.StatusId).ToString();
        complaint.StatusId =
            (int)ComplaintStatusEnum.Assigned;

        await _complaintRepository
            .UpdateAsync(complaint);

        _logger.LogInformation(
            "Complaint {ComplaintId} assigned to Employee {EmployeeId}",
            complaintId,
            employee.EmployeeId);
        
        await _historyRepository.AddAsync(
            new ComplaintHistory()
            {
                ComplaintId = complaintId,
                Action = $"Assigned to employee{employee.User.Name}",
                Details = "Succesfully Assigned to Employee",
                ChangedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            }
        );

        await _notificationService.CreateAsync(
            complaint.UserId,
            $"Your complaint \"{complaint.Title}\" has been assigned to {employee.User.Name}.",
            complaintId);

        var currentUserId1 = _currentUserService.GetUserId();
        if (employee.UserId != currentUserId1)
        {
            await _notificationService.CreateAsync(
                employee.UserId,
                $"Complaint \"{complaint.Title}\" has been assigned to you.",
                complaintId);
        }

        return new AssignComplaintResponseDto
        {
            IsAssigned = true,

            Message =
                $"Complaint assigned to {employee.User!.Name}"
        };
    }
    
    public async Task<AssignComplaintResponseDto> AssignComplaintToEmployeeAsync(
    int complaintId,
    int employeeId)
{
    var complaint =
        await _complaintRepository
            .GetByIdAsync(complaintId);

    if (complaint == null)
    {
        throw new NotFoundException(
            "The requested complaint could not be found.");
    }

    var employee =
        await _employeeRepository
            .GetByIdAsync(employeeId);

    if (employee == null)
    {
        throw new NotFoundException("The specified employee could not be found.");
    }

    if (!employee.IsActive)
    {
        throw new BadRequestException("Employee is inactive");
    }

    if (employee.Designation != EmployeeDesignationEnum.Employee)
    {
        throw new BusinessRuleException("Complaints can only be assigned to employees with the designation 'Employee'.");
    }

    if (complaint.PriorityId == null)
    {
        throw new BadRequestException("Priority must be assigned before assigning an employee");
    }

    if (complaint.EmployeeId != null)
    {
        throw new BadRequestException("Complaint is already assigned");
    }

    if (complaint.ComplaintCategory == null)
    {
        throw new ValidationException("Complaint category information is missing");
    }

    if (employee.Department == null)
    {
        throw new ValidationException("Employee department information is missing");
    }

    if (employee.User == null)
    {
        throw new ValidationException("Employee user information is missing");
    }

    if (complaint.CategoryId != employee.DepartmentId)
    {
        throw new BusinessRuleException($"Complaint can only be assigned to employees from the {complaint.ComplaintCategory.Categoryname} department");
    }

    complaint.EmployeeId = employee.EmployeeId;

    complaint.StatusId =
        (int)ComplaintStatusEnum.Assigned;

    await _complaintRepository
        .UpdateAsync(complaint);

        await _historyRepository.AddAsync(
            new ComplaintHistory
            {
                ComplaintId = complaintId,
                Action = $"Assigned to employee {employee.User.Name}",
                Details = "Complaint successfully assigned to employee",
                ChangedBy = "Admin",
                CreatedAt = DateTime.UtcNow
            });

        await _notificationService.CreateAsync(
            complaint.UserId,
            $"Your complaint \"{complaint.Title}\" has been assigned to {employee.User.Name}.",
            complaintId);

        var currentUserId = _currentUserService.GetUserId();
        if (employee.UserId != currentUserId)
        {
            await _notificationService.CreateAsync(
                employee.UserId,
                $"Complaint \"{complaint.Title}\" has been assigned to you.",
                complaintId);
        }

        return new AssignComplaintResponseDto
        {
            IsAssigned = true,

            Message =
                $"Complaint assigned to {employee.User!.Name}"
        };
    }

    public async Task UpdateComplaintStatusAsync(
        int complaintId,
        UpdateComplaintStatusRequestDto request)
    {
        var complaint = await _complaintRepository
            .GetByIdAsync(complaintId);

        if (complaint == null)
        {
            throw new NotFoundException(
                "Complaint not found");
        }

        // Validate Status Enum
        if (!Enum.IsDefined(
                typeof(ComplaintStatusEnum),
                request.Status))
        {
            throw new BadRequestException(
                "Invalid status");
        }
        var role = _currentUserService.GetRole();
        var userId = _currentUserService.GetUserId();
        var employeeId = _currentUserService.GetEmployeeId();

        if (role == "Employee" && employeeId != complaint.EmployeeId)
        {
            throw new UnauthorizedAccessException("You are not auhorised To change the Status fo this complaint.");
        }
        
        if (( role == "User" && userId != complaint.UserId))
        {
            throw new UnauthorizedAccessException("You are not authorised to Change the Status of this Complaint");
        }

        // Prevent Same Status Update
        if (complaint.StatusId ==
            (int)request.Status)
        {
            throw new BadRequestException(
                "Complaint is already in this status");
        }

        

        bool isValidTransition = false;

        if (role == "Admin")
        {
            isValidTransition = true;
        }
        else if (role == "Employee")
        {
            isValidTransition =
                (complaint.StatusId ==
                 (int)ComplaintStatusEnum.Assigned
                 &&
                 request.Status ==
                 ComplaintStatusEnum.InProgress)

                ||

                (complaint.StatusId ==
                 (int)ComplaintStatusEnum.InProgress
                 &&
                 request.Status ==
                 ComplaintStatusEnum.Resolved)
                ||
                (complaint.StatusId ==
                (int)ComplaintStatusEnum.Reopened
                &&
                request.Status ==
                ComplaintStatusEnum.Resolved);
                
        }
        else if (role == "User")
        {
            isValidTransition =
                (complaint.StatusId ==
                 (int)ComplaintStatusEnum.Resolved
                 &&
                 request.Status ==
                 ComplaintStatusEnum.Closed)

                ||

                (complaint.StatusId ==
                 (int)ComplaintStatusEnum.Resolved
                 &&
                 request.Status ==
                 ComplaintStatusEnum.Reopened);
        }

        if (!isValidTransition)
        {
            throw new BadRequestException(
                "Invalid status transition");
        }

        var oldStatus =
            ((ComplaintStatusEnum)complaint.StatusId)
            .ToString();

        complaint.StatusId =
            (int)request.Status;

        var user = await _userRepository.GetByIdAsync(userId);

        if (request.Status ==
            ComplaintStatusEnum.Resolved)
        {
            complaint.ResolvedAt =
                DateTime.UtcNow;
        }

        await _complaintRepository
            .UpdateAsync(complaint);

        await _historyRepository
            .AddAsync(
                new ComplaintHistory
                {
                    ComplaintId =
                        complaint.ComplaintId,

                    Action = $"Status Changed to {request.Status}",

                    Details = $"Status changed",

                    ChangedBy = user.Name,

                    CreatedAt =
                        DateTime.UtcNow
                });

        var newStatusName = request.Status.ToString();
        await _notificationService.CreateAsync(
            complaint.UserId,
            $"Status of your complaint \"{complaint.Title}\" changed from {oldStatus} to {newStatusName}.",
            complaintId);

        if (complaint.EmployeeId.HasValue)
        {
            var assignedEmployee = await _employeeRepository.GetByIdAsync(complaint.EmployeeId.Value);
            if (assignedEmployee != null && assignedEmployee.UserId != userId)
            {
                await _notificationService.CreateAsync(
                    assignedEmployee.UserId,
                    $"Status of complaint \"{complaint.Title}\" changed from {oldStatus} to {newStatusName}.",
                    complaintId);
            }
        }
    }

    public async Task<List<ComplaintTrackingDto>>
        GetComplaintTrackingAsync(int complaintId)
    {
        var complaint =
            await _complaintRepository
                .GetByIdAsync(complaintId);

        if (complaint == null)
        {
            throw new NotFoundException(
                "Complaint not found");
        }

        var role =
            _currentUserService.GetRole();

        bool canView = false;

        if (role == RolesEnum.Admin.ToString())
        {
            canView = true;
        }
        else if (role == RolesEnum.User.ToString())
        {
            canView =
                complaint.UserId ==
                _currentUserService.GetUserId();
        }
        else if (role == RolesEnum.Employee.ToString())
        {
            canView =
                complaint.EmployeeId ==
                _currentUserService.GetEmployeeId();
        }

        if (!canView)
        {
            throw new UnauthorizedAccessException(
                "You cannot view this complaint.");
        }

        var history =
            await _historyRepository
                .GetComplaintHistoryByComplaintIdAsync(
                    complaintId);

        if (!history.Any())
        {
            throw new NotFoundException(
                $"No tracking history found for complaint {complaintId}");
        }

        return history
            .Select(h => new ComplaintTrackingDto
            {
                Action = h.Action,
                Description = h.Details,
                PerformedBy = h.ChangedBy,
                CreatedAt = h.CreatedAt
            })
            .ToList();
    }

    public async Task<ComplaintDetailsDto> GetComplaintDetailsById(int complaintId)
    {
        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
    
        if (complaint == null)
        {
            throw new NotFoundException("Complaint not found.");
        }
    
        Employee? employee = null;
    
        if (complaint.EmployeeId.HasValue)
        {
            employee = await _employeeRepository
                .GetByIdAsync(complaint.EmployeeId.Value);
        }
    
        var role = _currentUserService.GetRole();

        bool canView = false;

        if (role == RolesEnum.Admin.ToString())
        {
            canView = true;
        }
        else if (role == RolesEnum.Employee.ToString())
        {
            canView =
                complaint.EmployeeId ==
                _currentUserService.GetEmployeeId();
        }
        else if (role == RolesEnum.User.ToString())
        {
            canView =
                complaint.UserId ==
                _currentUserService.GetUserId();
        }

        if (!canView)
        {
            throw new UnauthorizedAccessException(
                "You cannot view the details of this complaint.");
        }
        
        return new ComplaintDetailsDto
        {
            Title = complaint.Title,

            Description = complaint.Description,

            Priority = complaint.PriorityId.HasValue
                ? ((ComplaintPriorityEnum)complaint.PriorityId.Value).ToString()
                : null,

            Status =
                ((ComplaintStatusEnum)complaint.StatusId)
                .ToString(),

            Assignedto =
                employee?.User?.Name,

            Comments =
                complaint.Comments
                    .Select(c => new CommentDto
                    {
                        CommentId = c.CommentId,

                        CommentText = c.Message,

                        CommentedBy = c.CommentedBy,

                        CreatedAt = c.CreatedAt
                    })
                    .ToList(),

            Attachments =
                complaint.ComplaintAttachments
                    .Select(a => new AttachmentDto
                    {
                        AttachmentId = a.AttachmentId,

                        FileName = a.FileName,

                        FilePath = a.FilePath,

                        UploadedAt = a.UploadedAt
                    })
                    .ToList(),

            CreatedAt =
                complaint.CreatedAt,

            DueDate =
                complaint.DueDate,

            CreatedByUserId = complaint.UserId,

            AssignedEmployeeId = complaint.EmployeeId,

            CategoryId = complaint.CategoryId,

            CategoryName = ((ComplaintCategoryEnum)complaint.CategoryId).ToString()
        };
    }

    public async Task<(string FilePath, string FileName)> GetAttachmentAsync(int complaintId, int attachmentId)
    {
        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null)
        {
            throw new NotFoundException("Complaint not found.");
        }

        var role = _currentUserService.GetRole();
        bool canView = false;

        if (role == RolesEnum.Admin.ToString())
        {
            canView = true;
        }
        else if (role == RolesEnum.Employee.ToString())
        {
            canView = complaint.EmployeeId == _currentUserService.GetEmployeeId();
        }
        else if (role == RolesEnum.User.ToString())
        {
            canView = complaint.UserId == _currentUserService.GetUserId();
        }

        if (!canView)
        {
            throw new UnauthorizedAccessException("You cannot view the attachments of this complaint.");
        }

        var attachment = complaint.ComplaintAttachments?.FirstOrDefault(a => a.AttachmentId == attachmentId);
        if (attachment == null)
        {
            throw new NotFoundException("Attachment not found.");
        }

        return (attachment.FilePath, attachment.FileName);
    }
}