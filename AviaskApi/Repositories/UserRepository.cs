using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AviaskApi.Repositories;

public class UserRepository : IAviaskRepository<AviaskUser, Guid>
{
    private readonly AviaskApiContext _context = default!;
    private readonly UserManager<AviaskUser> _userManager = default!;

    public UserRepository(AviaskApiContext ctx, UserManager<AviaskUser> userManager)
    {
        _context = ctx;
        _userManager = userManager;
    }

    [Obsolete]
    public Task CreateAsync(AviaskUser entity)
    {
        throw new NotImplementedException("Please use CreateUserAsync instead");
    }

    public virtual async Task DeleteAsync(AviaskUser entity)
    {
        await _userManager.DeleteAsync(entity);
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _userManager.FindByIdAsync(id.ToString()) is not null;
    }

    public virtual async Task<AviaskUser[]> GetAllAsync()
    {
        return await _context.AviaskUsers.ToArrayAsync();
    }

    public virtual async Task<AviaskUser?> GetByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return null;

        user.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        return user;
    }

    public virtual IQueryable<AviaskUser> GetQuery()
    {
        return _context.AviaskUsers.Include(u => u.Publications);
    }

    public virtual async Task UpdateAsync(AviaskUser entity)
    {
        await _userManager.UpdateAsync(entity);
    }

    public virtual async Task<IEnumerable<string>> CreateUserAsync(AviaskUser entity, string password)
    {
        entity.CreatedAt = DateTime.UtcNow;
        var result = await _userManager.CreateAsync(entity, password);

        if (!result.Succeeded) return result.Errors.Select(e => e.Description);

        await _userManager.AddToRoleAsync(entity, "member");

        return Enumerable.Empty<string>();
    }

    public virtual async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) is not null;
    }

    public virtual async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username) is not null;
    }

    public virtual async Task<AviaskUser?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public virtual async Task<AviaskUser?> GetByUsernameAsync(string username)
    {
        return await _context.AviaskUsers.Where(u => u.UserName == username).FirstOrDefaultAsync();
    }

    public virtual async Task<AviaskUser[]> GetMostPublicationUsers(int max)
    {
        return await _context.AviaskUsers
            .Include(u => u.Publications.Where(q => q.Status == QuestionStatus.ACCEPTED))
            .OrderByDescending(u => u.Publications.Count)
            .Take(max)
            .ToArrayAsync();
    }
}
