namespace AuthService.Domain.Entities;

public class LoginHistory: BaseEntity
{
    public Guid UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    
    // Navigation
    public User User { get; set; } = null!;
}