using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Application.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAsync(User user, OtpPurpose purpose);
    Task<bool> VerifyAsync(User user, string code, OtpPurpose purpose);
}