using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using Microsoft.IdentityModel.Tokens;

namespace ComplaintManagementSystem.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private  readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<TokenService> _logger;
    public TokenService(IConfiguration config, 
        IEmployeeRepository employeeRepository,
        ILogger<TokenService> logger)
    {
        _config = config;
        _logger = logger;
        _employeeRepository = employeeRepository;
    }

    public async Task<string> CreateToken(User user)
    {   
        _logger.LogInformation("Generating Token For User {Email}", user.Email);
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Name),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        if (user.Role == RolesEnum.Employee.ToString())
        {   
            var employee = await _employeeRepository.GetByEmailAsync(user.Email);
            claims.Add(
                new Claim(
                    "EmployeeId",
                    employee.EmployeeId.ToString()));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _config["Jwt:Key"]));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);
        _logger.LogInformation("Token Created For User {Email}", user.Email);
        return new JwtSecurityTokenHandler()
            .WriteToken(token);
        
    }
    
}