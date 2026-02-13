using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Data.Configurations;

public class OtpCodeConfiguration:IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Code)
            .IsRequired()
            .HasMaxLength(6);
        
        // Enum-u string kimi saxla (database-də "Login" yazılır, 0/1 yox)
        builder.Property(o => o.Purpose)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        // ↑ Niyə string? "1" görsən nə olduğunu bilmirsən,
        //   "Register" görsən dərhal başa düşürsən
        
        builder.Property(o => o.ExpiresAt)
            .IsRequired();
        
        // Composite Index — bu sorğu tez-tez olacaq:
        // "Bu user-in Login üçün aktiv OTP-si varmı?"
        builder.HasIndex(o => new { o.UserId, o.Purpose });
        
        // Computed property-lər DB-də saxlanmamalıdır
        builder.Ignore(o => o.IsExpired);
        builder.Ignore(o => o.IsLocked);
        builder.Ignore(o => o.IsValid);
    }
}