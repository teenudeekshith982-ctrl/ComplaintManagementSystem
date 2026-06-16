using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {   
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<RegisterResponseDto>  Register( RegisterRequestDto registerRequest)
        {
            return await _authService.RegisterUser(registerRequest);
        }

        [HttpPost("Login")]
        public async  Task<LoginResponseDto> Login(LoginRequestDto loginRequest)
        {
            return await _authService.LoginUser(loginRequest);
        }
    }
}
