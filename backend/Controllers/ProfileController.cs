using System;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagementSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;

        public ProfileController(
            ICurrentUserService currentUserService,
            IUserService userService)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            int userId = _currentUserService.GetUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            int userId = _currentUserService.GetUserId();
            var updatedProfile = await _userService.UpdateProfileAsync(userId, request);
            return Ok(new
            {
                Message = "Profile updated successfully.",
                updatedProfile.UserId,
                updatedProfile.Name,
                updatedProfile.Phone
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            int userId = _currentUserService.GetUserId();
            await _userService.ChangePasswordAsync(userId, request);
            return Ok(new { Message = "Password changed successfully." });
        }
    }
}
