using AviaskApi.Entities;
using AviaskApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AviaskApi.Repositories;

public class QuestionReportRepository : IAviaskRepository<QuestionReport, Guid>
{
    private readonly AviaskApiContext _context;

    public QuestionReportRepository(AviaskApiContext ctx)
    {
        _context = ctx;
    }

    public virtual IQueryable<QuestionReport> GetQuery()
    {
        return _context.QuestionReports
            .Include(q => q.Question)
            .ThenInclude(q => q.QuestionAnswers)
            .Include(q => q.Issuer)
            .AsQueryable();
    }

    public virtual async Task<QuestionReport[]> GetAllAsync()
    {
        return await GetQuery().ToArrayAsync();
    }

    public virtual async Task CreateAsync(QuestionReport entity)
    {
        await _context.QuestionReports.AddAsync(entity);

        await _context.SaveChangesAsync();
    }

    public virtual async Task<QuestionReport?> GetByIdAsync(Guid id)
    {
        return await GetQuery().Where(q => q.Id == id).FirstOrDefaultAsync();
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.QuestionReports.Where(q => q.Id == id).FirstOrDefaultAsync() is not null;
    }

    public virtual async Task UpdateAsync(QuestionReport entity)
    {
        _context.QuestionReports.Update(entity);

        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(QuestionReport entity)
    {
        _context.QuestionReports.Remove(entity);

        await _context.SaveChangesAsync();
    }
}