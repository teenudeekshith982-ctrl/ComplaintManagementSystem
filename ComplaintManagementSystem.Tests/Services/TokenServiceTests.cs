using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ComplaintManagementSystem.Enums;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ComplaintManagementSystem.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<ILogger<TokenService>> _loggerMock;

    private readonly TokenService _service;

    public TokenServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<TokenService>>();

        _configurationMock
            .Setup(x => x["Jwt:Key"])
            .Returns("ThisIsASuperSecretKeyForUnitTesting123456");

        _configurationMock
            .Setup(x => x["Jwt:Issuer"])
            .Returns("ComplaintSystem");

        _configurationMock
            .Setup(x => x["Jwt:Audience"])
            .Returns("ComplaintSystemUsers");

        _service = new TokenService(
            _configurationMock.Object,
            _employeeRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateToken_ShouldReturnToken_ForUser()
    {
        // Arrange

        var user = new User
        {
            UserId = 1,
            Name = "Teenu",
            Email = "teenu@test.com",
            Role = RolesEnum.User.ToString()
        };

        // Act

        var token =
            await _service.CreateToken(user);

        // Assert

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateToken_ShouldContainUserClaims()
    {
        // Arrange

        var user = new User
        {
            UserId = 1,
            Name = "Teenu",
            Email = "teenu@test.com",
            Role = RolesEnum.User.ToString()
        };

        // Act

        var token =
            await _service.CreateToken(user);

        var handler =
            new JwtSecurityTokenHandler();

        var jwt =
            handler.ReadJwtToken(token);

        // Assert

        jwt.Claims
            .First(x => x.Type == ClaimTypes.NameIdentifier)
            .Value
            .Should()
            .Be("1");

        jwt.Claims
            .First(x => x.Type == ClaimTypes.Name)
            .Value
            .Should()
            .Be("Teenu");

        jwt.Claims
            .First(x => x.Type == ClaimTypes.Role)
            .Value
            .Should()
            .Be("User");
    }

    [Fact]
    public async Task CreateToken_ShouldIncludeEmployeeIdClaim_ForEmployee()
    {
        // Arrange

        var user = new User
        {
            UserId = 2,
            Name = "John",
            Email = "john@test.com",
            Role = RolesEnum.Employee.ToString()
        };

        var employee = new Employee
        {
            EmployeeId = 100,
            UserId = 2
        };

        _employeeRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(employee);

        // Act

        var token =
            await _service.CreateToken(user);

        var handler =
            new JwtSecurityTokenHandler();

        var jwt =
            handler.ReadJwtToken(token);

        // Assert

        jwt.Claims
            .First(x => x.Type == "EmployeeId")
            .Value
            .Should()
            .Be("100");
    }

    [Fact]
    public async Task CreateToken_ShouldNotIncludeEmployeeIdClaim_ForNormalUser()
    {
        // Arrange

        var user = new User
        {
            UserId = 1,
            Name = "Teenu",
            Email = "teenu@test.com",
            Role = RolesEnum.User.ToString()
        };

        // Act

        var token =
            await _service.CreateToken(user);

        var handler =
            new JwtSecurityTokenHandler();

        var jwt =
            handler.ReadJwtToken(token);

        // Assert

        jwt.Claims
            .Any(x => x.Type == "EmployeeId")
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task CreateToken_ShouldCreateValidJwtToken()
    {
        // Arrange

        var user = new User
        {
            UserId = 1,
            Name = "Admin",
            Email = "admin@test.com",
            Role = RolesEnum.Admin.ToString()
        };

        // Act

        var token =
            await _service.CreateToken(user);

        var handler =
            new JwtSecurityTokenHandler();

        // Assert

        handler.CanReadToken(token)
            .Should()
            .BeTrue();
    }
}