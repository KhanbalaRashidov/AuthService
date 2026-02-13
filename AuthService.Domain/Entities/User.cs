namespace AuthService.Domain.Entities;

public class User:BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // Security
    public bool IsEmailConfirmed { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    
    public string Role { get; set; } = "User";
    
    // Navigation
    public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    public ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
    
    // Domain Logic
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;
}