using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IAuthService
{
    public Task<RegisterResponseDto> RegisterUser (RegisterRequestDto request);
    public Task<LoginResponseDto> LoginUser (LoginRequestDto request);
    public Task<RegisterResponseDto> SetupAdmin(RegisterRequestDto request);
}