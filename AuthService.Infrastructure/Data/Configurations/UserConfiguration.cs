using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Data.Configurations;

public class UserConfiguration:IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(u => u.Email)
            .IsUnique();
        // ↑ Eyni email ilə 2 nəfər qeydiyyat ola bilməz
        // Database səviyyəsində qoruma — kod xətası olsa belə DB rədd edəcək
        
        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique();
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("User");
        
        // Relationship: User → OtpCodes (one-to-many)
        builder.HasMany(u => u.OtpCodes)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        // ↑ User silinəndə OTP kodları da silinir
        
        // Relationship: User → LoginHistories (one-to-many)
        builder.HasMany(u => u.LoginHistories)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}