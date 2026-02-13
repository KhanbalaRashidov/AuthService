using System.Security.Cryptography;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Infrastructure.Services;

public class OtpService:IOtpService
{
    private readonly IUnitOfWork _uow;

    public OtpService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task<string> GenerateAsync(User user, OtpPurpose purpose)
    {
        var activeOtps = await _uow.OtpCodes.FindAsync(
            o => o.UserId == user.Id 
                 && o.Purpose == purpose 
                 && !o.IsUsed);
        
        foreach (var oldOtp in activeOtps)
        {
            oldOtp.IsUsed = true;
            _uow.OtpCodes.Update(oldOtp);
        }
        
        // 2. Kriptoqrafik təhlükəsiz kod yarat
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        
        // 3. Yeni OTP entity yarat
        var otp = new OtpCode
        {
            UserId = user.Id,
            Code = code,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0
        };
        
        await _uow.OtpCodes.AddAsync(otp);
        // SaveChanges burada ÇAĞIRILMIR — AuthService-də çağırılacaq (UoW pattern!)
        
        return code;
    }

    public async Task<bool> VerifyAsync(User user, string code, OtpPurpose purpose)
    {
        var otps = await _uow.OtpCodes.FindAsync(
            o => o.UserId == user.Id 
                 && o.Purpose == purpose 
                 && !o.IsUsed);
        
        var otp = otps.OrderByDescending(o => o.CreatedAt).FirstOrDefault();
        
        // OTP tapılmadı
        if (otp == null)
            return false;
        
        // Vaxtı keçib?
        if (otp.IsExpired)
            return false;
        
        // 3 dəfədən çox səhv daxil edilib?
        if (otp.IsLocked)
            return false;
        
        // Kod düzgündür?
        if (otp.Code != code)
        {
            otp.AttemptCount++;
            _uow.OtpCodes.Update(otp);
            // SaveChanges AuthService-də
            return false;
        }
        
        // ✅ Uğurlu — kodu istifadə olunmuş kimi işarələ
        otp.IsUsed = true;
        _uow.OtpCodes.Update(otp);
        
        return true;
    }
}