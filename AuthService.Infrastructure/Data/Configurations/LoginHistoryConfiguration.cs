using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Data.Configurations;

public class LoginHistoryConfiguration:IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.IpAddress)
            .IsRequired()
            .HasMaxLength(45);
        // ↑ 45 char — IPv6 ən uzun format: 
        //   "ffff:ffff:ffff:ffff:ffff:ffff:255.255.255.255"
        
        builder.Property(l => l.UserAgent)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(l => l.FailureReason)
            .HasMaxLength(200);
        
        // Index — "Bu user-in son girişləri" sorğusu üçün
        builder.HasIndex(l => l.UserId);
        
        // Index — "Bugün neçə uğursuz giriş olub?" sorğusu üçün
        builder.HasIndex(l => new { l.UserId, l.IsSuccessful });
    }
}