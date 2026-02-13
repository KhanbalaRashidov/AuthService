using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class OtpCode: BaseEntity
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public int AttemptCount { get; set; } = 0;
    
    // Navigation
    public User User { get; set; } = null!;
    
    // Domain Logic
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsLocked => AttemptCount >= 3;
    public bool IsValid => !IsUsed && !IsExpired && !IsLocked;
}