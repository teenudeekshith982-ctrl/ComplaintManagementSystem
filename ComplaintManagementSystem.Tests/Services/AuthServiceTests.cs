using AutoMapper;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ComplaintManagementSystem.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;

    private readonly AuthService _authService;
    public AuthServiceTests()
    {
        _userRepoMock =
            new Mock<IUserRepository>();

        _tokenServiceMock =
            new Mock<ITokenService>();

        _employeeRepoMock =
            new Mock<IEmployeeRepository>();

        _mapperMock =
            new Mock<IMapper>();

        _loggerMock =
            new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _userRepoMock.Object,
            _tokenServiceMock.Object,
            _employeeRepoMock.Object,
            _loggerMock.Object,
            _mapperMock.Object);
        
    }
    
    [Fact]
    public async Task LoginUser_ShouldThrowUnauthorizedException_WhenUserNotFound()
    {
        var request = new LoginRequestDto
        {
            Email = "john@gmail.com",
            Password = "Password123"
        };

        _userRepoMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        Func<Task> action =
            () => _authService.LoginUser(request);

        
        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginUser_ShouldThrowUnAuthorizedException_WhenPasswordNotFound()
    {
        var request = new LoginRequestDto
        {
            Email = "john@gmail.com",
            Password = "Password123"
        };
        
        var user = new User
        {
            UserId = 1,
            Email = "john@gmail.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    "CorrectPassword")
        };

        _userRepoMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        
        Func<Task> action = ()=> _authService.LoginUser(request);
        
        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }


    [Fact]
    public async Task LoginUser_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var request = new LoginRequestDto
        {
            Email = "john@gmail.com",
            Password = "Password123"
        };
        
        var user = new User
        {
            UserId = 1,
            Email = "john@gmail.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    "Password123")
        };
        
        _userRepoMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);
        
        _tokenServiceMock.Setup(x=>x.CreateToken(user))
            .ReturnsAsync("fakeToken");
        
         var result = await _authService.LoginUser(request);
        
        result.Should().NotBeNull();
        result.Token.Should()
            .Be("fakeToken");
    }
    
    
}