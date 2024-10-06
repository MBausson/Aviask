using AviaskApi.Entities;
using AviaskApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AviaskApi.Repositories;

public class AttachmentRepository : IAviaskRepository<Attachment, Guid>
{
    private readonly AviaskApiContext _context;

    public AttachmentRepository(AviaskApiContext context)
    {
        _context = context;
    }

    public virtual IQueryable<Attachment> GetQuery()
    {
        return _context.Attachments;
    }

    public virtual async Task<Attachment[]> GetAllAsync()
    {
        return await GetQuery().ToArrayAsync();
    }

    public virtual async Task CreateAsync(Attachment entity)
    {
        await _context.Attachments.AddAsync(entity);

        await _context.SaveChangesAsync();
    }

    public virtual async Task<Attachment?> GetByIdAsync(Guid id)
    {
        return await _context.Attachments.FirstOrDefaultAsync(i => i.Id == id);
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.Attachments.FirstOrDefaultAsync(i => i.Id == id) is null;
    }

    public virtual async Task UpdateAsync(Attachment entity)
    {
        _context.Attachments.Update(entity);

        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Attachment entity)
    {
        _context.Attachments.Remove(entity);

        await _context.SaveChangesAsync();
    }
}