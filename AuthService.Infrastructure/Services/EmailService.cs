using AuthService.Application.Interfaces;

namespace AuthService.Infrastructure.Services;

public class EmailService:IEmailService
{
    public Task SendOtpAsync(string email, string otpCode, string purpose)
    {
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine($"  📧 EMAIL GÖNDƏRİLDİ");
        Console.WriteLine($"  To:      {email}");
        Console.WriteLine($"  Purpose: {purpose}");
        Console.WriteLine($"  OTP:     {otpCode}");
        Console.WriteLine($"  Expires: 5 dəqiqə");
        Console.WriteLine("═══════════════════════════════════════");
        
        return Task.CompletedTask;
    }
}