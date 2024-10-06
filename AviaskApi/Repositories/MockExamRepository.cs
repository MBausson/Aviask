using AviaskApi.Entities;
using AviaskApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AviaskApi.Repositories;

public class MockExamRepository : IAviaskRepository<MockExam, Guid>
{
    private readonly AviaskApiContext _context;

    public MockExamRepository(AviaskApiContext context)
    {
        _context = context;
    }

    public virtual IQueryable<MockExam> GetQuery()
    {
        return _context.MockExams
            .Include(m => m.User)
            .Include(m => m.AnswerRecords)
            .ThenInclude(a => a.Question);
    }

    public virtual async Task<MockExam[]> GetAllAsync()
    {
        return await GetQuery().ToArrayAsync();
    }

    public virtual async Task CreateAsync(MockExam entity)
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<MockExam?> GetByIdAsync(Guid id)
    {
        return await _context.MockExams.Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.MockExams.FirstOrDefaultAsync(m => m.Id == id) is not null;
    }

    public virtual async Task UpdateAsync(MockExam entity)
    {
        _context.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(MockExam entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task FinishMockExamAsync(MockExam entity)
    {
        entity.Status = MockExamStatus.FINISHED;
        entity.CorrectnessRatio = entity.ProcessCorrectnessRatio();
        entity.EndedAt = DateTime.UtcNow;

        await UpdateAsync(entity);
    }

    public virtual async Task<MockExam?> GetOngoingByUserIdAsync(Guid userId)
    {
        return await GetQuery().FirstOrDefaultAsync(m => m.Status == MockExamStatus.ONGOING && m.UserId == userId);
    }
}