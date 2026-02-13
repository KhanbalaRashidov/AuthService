using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Infrastructure.Services;

public class AuthenticationService:IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;

    public AuthenticationService(
        IUnitOfWork uow,
        IOtpService otpService,
        IJwtService jwtService,
        IEmailService emailService)
    {
        _uow = uow;
        _otpService = otpService;
        _jwtService = jwtService;
        _emailService = emailService;
    }
    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            throw new BadHttpRequestException("Şifrələr uyğun deyil");
        
        // 2. Email artıq mövcuddur?
        var existingUser = await _uow.Users.FirstOrDefaultAsync(
            u => u.Email == dto.Email);
        
        if (existingUser != null)
            throw new BadHttpRequestException("Bu email artıq qeydiyyatdan keçib");
        
        // 3. Telefon artıq mövcuddur?
        var existingPhone = await _uow.Users.FirstOrDefaultAsync(
            u => u.PhoneNumber == dto.PhoneNumber);
        
        if (existingPhone != null)
            throw new BadHttpRequestException("Bu telefon nömrəsi artıq qeydiyyatdan keçib");
        
        // 4. User yarat
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower().Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsEmailConfirmed = false,
            IsActive = true
        };
        await _uow.Users.AddAsync(user);
        
        // 5. OTP yarat və göndər
        var otpCode = await _otpService.GenerateAsync(user, OtpPurpose.Register);
        
        // 6. Hamısını birlikdə saxla (UoW pattern!)
        await _uow.SaveChangesAsync();
        
        // 7. Email göndər (DB save-dən SONRA)
        await _emailService.SendOtpAsync(user.Email, otpCode, "Qeydiyyat təsdiqi");
        
        return new RegisterResponseDto
        {
            Message = "Qeydiyyat uğurlu! Email-inizə göndərilən OTP kodu daxil edin.",
            Email = user.Email
        };

    }

    public async Task<RegisterResponseDto> LoginAsync(LoginDto dto)
    {
         var user = await _uow.Users.FirstOrDefaultAsync(
            u => u.PhoneNumber == dto.PhoneNumber.ToLower().Trim());
        
        if (user == null)
            throw new BadHttpRequestException("Email və ya şifrə yanlışdır");
        
        // 2. Hesab aktivdir?
        if (!user.IsActive)
            throw new BadHttpRequestException("Hesab deaktiv edilib");
        
        // 3. Hesab kilidlidir?
        if (user.IsLockedOut)
        {
            var remaining = (user.LockoutEnd!.Value - DateTime.UtcNow).Minutes;
            throw new BadHttpRequestException(
                $"Hesab kilidlidir. {remaining} dəqiqə sonra yenidən cəhd edin");
        }
        
        // 4. Şifrə düzgündür?
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            
            // 5 dəfə səhv → hesabı kilidlə
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }
            
            _uow.Users.Update(user);
            
            // LoginHistory — uğursuz
            await _uow.LoginHistories.AddAsync(new LoginHistory
            {
                UserId = user.Id,
                IpAddress = "127.0.0.1", // Controller-dən gələcək
                UserAgent = "Unknown",
                IsSuccessful = false,
                FailureReason = "Yanlış şifrə"
            });
            
            await _uow.SaveChangesAsync();
            throw new BadHttpRequestException("Email və ya şifrə yanlışdır");
        }
        
        // 5. Email təsdiqlənib?
        if (!user.IsEmailConfirmed)
            throw new BadHttpRequestException("Əvvəlcə email-inizi təsdiqləyin");
        
        // 6. OTP yarat və göndər
        var otpCode = await _otpService.GenerateAsync(user, OtpPurpose.Login);
        
        // 7. Failed attempts sıfırla
        user.FailedLoginAttempts = 0;
        _uow.Users.Update(user);
        
        await _uow.SaveChangesAsync();
        await _emailService.SendOtpAsync(user.Email, otpCode, "Giriş təsdiqi");
        
        return new RegisterResponseDto
        {
            Message = "OTP kodu email-inizə göndərildi",
            Email = user.Email
        };
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto)
    {
        // 1. User tap
        var user = await _uow.Users.FirstOrDefaultAsync(
            u => u.Email == dto.Email.ToLower().Trim());
        
        if (user == null)
            throw new BadHttpRequestException("İstifadəçi tapılmadı");
        
        // 2. Purpose parse et
        if (!Enum.TryParse<OtpPurpose>(dto.Purpose, true, out var purpose))
            throw new BadHttpRequestException("Yanlış OTP məqsədi");
        
        // 3. OTP yoxla
        var isValid = await _otpService.VerifyAsync(user, dto.Code, purpose);
        
        if (!isValid)
        {
            await _uow.SaveChangesAsync(); // AttemptCount yeniləmək üçün
            throw new BadHttpRequestException("OTP kodu yanlışdır və ya vaxtı keçib");
        }
        
        // 4. Purpose-ə görə əməliyyat
        switch (purpose)
        {
            case OtpPurpose.Register:
                user.IsEmailConfirmed = true;
                _uow.Users.Update(user);
                break;
                
            case OtpPurpose.Login:
                // LoginHistory — uğurlu
                await _uow.LoginHistories.AddAsync(new LoginHistory
                {
                    UserId = user.Id,
                    IpAddress = "127.0.0.1",
                    UserAgent = "Unknown",
                    IsSuccessful = true,
                    FailureReason = null
                });
                break;
        }
        
        // 5. JWT token yarat
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        await _uow.SaveChangesAsync();
        
        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsEmailConfirmed = user.IsEmailConfirmed
            }
        };
    }
}