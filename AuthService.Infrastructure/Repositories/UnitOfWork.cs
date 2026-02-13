using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AuthService.Infrastructure.Repositories;

public class UnitOfWork:IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private IRepository<User>? _users;
    private IRepository<OtpCode> _otpCodes;
    private IRepository<LoginHistory> _loginHistories;
    
    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<User> Users =>_users ??= new Repository<User>(_dbContext);
    public IRepository<OtpCode> OtpCodes => _otpCodes ??= new Repository<OtpCode>(_dbContext);
    public IRepository<LoginHistory> LoginHistories => _loginHistories ??= new Repository<LoginHistory>(_dbContext);
    public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();
    public void Dispose()=>_dbContext.Dispose();
}