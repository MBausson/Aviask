using AviaskApi.Entities;
using AviaskApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AviaskApi.Repositories;

public class RecoveryTokenRepository : IAviaskRepository<RecoveryToken, Guid>
{
    private readonly AviaskApiContext _context;

    public RecoveryTokenRepository(AviaskApiContext ctx)
    {
        _context = ctx;
    }

    public virtual IQueryable<RecoveryToken> GetQuery()
    {
        return _context.RecoveryTokens.Include(r => r.User);
    }

    public virtual async Task<RecoveryToken[]> GetAllAsync()
    {
        return await _context.RecoveryTokens
            .Include(r => r.User)
            .ToArrayAsync();
    }

    public virtual async Task CreateAsync(RecoveryToken entity)
    {
        await _context.RecoveryTokens.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<RecoveryToken?> GetByIdAsync(Guid id)
    {
        return await _context.RecoveryTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.RecoveryTokens.FirstOrDefaultAsync(r => r.Id == id) is null;
    }

    public virtual async Task UpdateAsync(RecoveryToken entity)
    {
        _context.RecoveryTokens.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(RecoveryToken entity)
    {
        _context.RecoveryTokens.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<RecoveryToken[]> GetByUser(Guid userId)
    {
        return await _context.RecoveryTokens.Where(r => r.UserId == userId).ToArrayAsync();
    }
}