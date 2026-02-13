using System.Linq.Expressions;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }
    public async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FindAsync(id);
    
    public async Task<IReadOnlyList<T>> GetAllAsync()
        => await _dbSet.ToListAsync();
    
    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);
    
    public void Update(T entity)
        => _dbSet.Update(entity);
    
    public void Delete(T entity)
        => _dbSet.Remove(entity);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.FirstOrDefaultAsync(predicate);
}