using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IUnitOfWork:IDisposable
{
    IRepository<User> Users { get; }
    IRepository<OtpCode> OtpCodes { get; }
    IRepository<LoginHistory> LoginHistories { get; }
    
    Task<int> SaveChangesAsync();
}