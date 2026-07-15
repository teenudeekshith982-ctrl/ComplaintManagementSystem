using AutoMapper;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Repositories;

namespace ComplaintManagementSystem.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<AuthService> _logger;
    private readonly IMapper _mapper;
    public  AuthService(IUserRepository userRepository,
        ITokenService tokenService,
        IEmployeeRepository employeeRepository,
        ILogger<AuthService> logger,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _employeeRepository = employeeRepository;
        _mapper =  mapper;
        _logger = logger;
    }

    public  async Task<RegisterResponseDto> RegisterUser(RegisterRequestDto request)
    {   
        _logger.LogInformation("Registering user with {Email}",request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            _logger.LogWarning("User with email {request.Email} already exists",request.Email);
            throw new ConflictException("An account with this email address already exists. Please try another email or log in.");
        }
        
        existingUser = await _userRepository.GetByPhoneAsync(request.Phone);
        if (existingUser != null)
        {   
            _logger.LogWarning("User with Phonenumber {Phonenum} already exists",request.Phone);
            throw new ConflictException("An account with this phone number already exists. Please verify your details.");
        }
        
        var user = _mapper.Map<User>(request);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.JoinedDate = DateTime.UtcNow;
        user.Role = RolesEnum.User.ToString();
        user.IsActive = true;
        

        var result = await _userRepository.AddAsync(user);
        _logger.LogInformation("User with userid {UserId} Registered Successfully", result.UserId);
        return new RegisterResponseDto
        {   
            UserId = result.UserId,
            Message = "Registered Sucessfully"
        };
    }

    public async Task<LoginResponseDto> LoginUser(LoginRequestDto request)
    {
        _logger.LogInformation("Logging with user {Email}", request.Email);
        var user = await _userRepository
            .GetByEmailAsync(request.Email);

        if (user == null)
        {   
            _logger.LogWarning($"User with email {request.Email} does not exist");
            throw new UnauthorizedAccessException("Invalid credentials.Please Check Your Email and Password.");
        }

        var isPasswordValid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!isPasswordValid)
        {
            _logger.LogWarning("User with {Email} entered incorrect password",request.Email);
            throw new UnauthorizedAccessException("Invalid credentials.Please Check Your Email and Password.");
        }
        
        _logger.LogInformation("Login Successful with user {Email}", request.Email);
       
        var token = await _tokenService.CreateToken(user);
        
        return new LoginResponseDto
        {
            Token = token
        };
    }
    
}