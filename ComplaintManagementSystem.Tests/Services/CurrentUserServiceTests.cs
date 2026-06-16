using System.Security.Claims;
using ComplaintManagementSystem.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ComplaintManagementSystem.Tests.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CurrentUserService _service;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock =
            new Mock<IHttpContextAccessor>();

        _service =
            new CurrentUserService(
                _httpContextAccessorMock.Object);
    }

    [Fact]
    public void GetUserId_ShouldReturnUserId()
    {
        // Arrange

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                "10")
        };

        var identity =
            new ClaimsIdentity(claims);

        var principal =
            new ClaimsPrincipal(identity);

        var context =
            new DefaultHttpContext();

        context.User = principal;

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        // Act

        var result =
            _service.GetUserId();

        // Assert

        result.Should().Be(10);
    }

    [Fact]
    public void GetRole_ShouldReturnRole()
    {
        // Arrange

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.Role,
                "Admin")
        };

        var identity =
            new ClaimsIdentity(claims);

        var principal =
            new ClaimsPrincipal(identity);

        var context =
            new DefaultHttpContext();

        context.User = principal;

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        // Act

        var result =
            _service.GetRole();

        // Assert

        result.Should().Be("Admin");
    }

    [Fact]
    public void GetEmployeeId_ShouldReturnEmployeeId()
    {
        // Arrange

        var claims = new List<Claim>
        {
            new Claim(
                "EmployeeId",
                "25")
        };

        var identity =
            new ClaimsIdentity(claims);

        var principal =
            new ClaimsPrincipal(identity);

        var context =
            new DefaultHttpContext();

        context.User = principal;

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        // Act

        var result =
            _service.GetEmployeeId();

        // Assert

        result.Should().Be(25);
    }

    [Fact]
    public void GetEmployeeId_ShouldReturnNull_WhenEmployeeClaimMissing()
    {
        // Arrange

        var identity =
            new ClaimsIdentity();

        var principal =
            new ClaimsPrincipal(identity);

        var context =
            new DefaultHttpContext();

        context.User = principal;

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        // Act

        var result =
            _service.GetEmployeeId();

        // Assert

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserName_ShouldReturnUserName()
    {
        // Arrange

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.Name,
                "Teenu")
        };

        var identity =
            new ClaimsIdentity(
                claims,
                "TestAuth");

        var principal =
            new ClaimsPrincipal(identity);

        var context =
            new DefaultHttpContext();

        context.User = principal;

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        // Act

        var result =
            _service.GetUserName();

        // Assert

        result.Should().Be("Teenu");
    }

    [Fact]
    public void GetUserName_ShouldReturnEmptyString_WhenNameMissing()
    {
        // Arrange

        var identity =
            new ClaimsIdentity();

        var principal =
            new ClaimsPrincipal(identity);

        var context =
            new DefaultHttpContext();

        context.User = principal;

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        // Act

        var result =
            _service.GetUserName();

        // Assert

        result.Should().BeEmpty();
    }
}