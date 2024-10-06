using AviaskApi.Entities;
using AviaskApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AviaskApi.Repositories;

public class AnswerRecordRepository : IAviaskRepository<AnswerRecord, Guid>
{
    private readonly AviaskApiContext _context;

    public AnswerRecordRepository(AviaskApiContext context)
    {
        _context = context;
    }

    public virtual IQueryable<AnswerRecord> GetQuery()
    {
        return _context.AnswerRecords
            .Include(q => q.User)
            .Include(q => q.Question)
            .ThenInclude(q => q.QuestionAnswers)
            .Include(q => q.MockExam)
            .AsQueryable();
    }

    public virtual async Task<AnswerRecord[]> GetAllAsync()
    {
        return await GetQuery().ToArrayAsync();
    }

    public virtual async Task CreateAsync(AnswerRecord entity)
    {
        entity.AnsweredAt = DateTime.UtcNow;
        await _context.AnswerRecords.AddAsync(entity);

        await _context.SaveChangesAsync();
    }

    public virtual async Task<AnswerRecord?> GetByIdAsync(Guid id)
    {
        return await _context.AnswerRecords
            .Where(q => q.Id == id)
            .Include(q => q.User)
            .Include(q => q.Question)
            .ThenInclude(q => q.QuestionAnswers)
            .FirstOrDefaultAsync();
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.AnswerRecords
            .Where(q => q.Id == id)
            .AnyAsync();
    }

    public virtual async Task UpdateAsync(AnswerRecord entity)
    {
        _context.AnswerRecords.Update(entity);

        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(AnswerRecord entity)
    {
        _context.AnswerRecords.Remove(entity);

        await _context.SaveChangesAsync();
    }

    public virtual async Task<AnswerRecord[]> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.AnswerRecords
            .Where(q => q.UserId == userId)
            .Include(q => q.User)
            .Include(q => q.Question)
            .ThenInclude(q => q.QuestionAnswers)
            .Include(q => q.Question.Publisher)
            .ToArrayAsync();
    }
}