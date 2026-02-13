using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
    Task<RegisterResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto);
}