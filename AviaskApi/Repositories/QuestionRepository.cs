using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Services.FreeQuestionsPool;
using Microsoft.EntityFrameworkCore;

namespace AviaskApi.Repositories;

public class QuestionRepository : IAviaskRepository<Question, Guid>
{
    private static readonly TimeSpan _cacheInterval = TimeSpan.FromMinutes(3);

    private static DateTime _lastCountFetch = DateTime.MinValue;
    private static int _countCache = -1;

    private readonly AviaskApiContext _context;

    public QuestionRepository(AviaskApiContext ctx)
    {
        _context = ctx;
    }

    private bool _fetchCount => DateTime.Now - _lastCountFetch >= _cacheInterval;

    public virtual async Task CreateAsync(Question entity)
    {
        entity.PublishedAt = DateTime.Now.ToUniversalTime();

        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Question entity)
    {
        _context.Remove(entity);

        //  TODO use cascade delete instead on the FK
        _context.QuestionAnswers.Remove(entity.QuestionAnswers);

        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.Questions.Where(q => q.Id == id).AnyAsync();
    }

    public virtual async Task<Question[]> GetAllAsync()
    {
        return await _context.Questions
            .Include(q => q.QuestionAnswers)
            .Include(q => q.Publisher)
            .ToArrayAsync();
    }

    public virtual async Task<Question?> GetByIdAsync(Guid id)
    {
        return await _context.Questions
            .Where(q => q.Id == id)
            .Include(q => q.QuestionAnswers)
            .Include(q => q.Publisher)
            .FirstOrDefaultAsync();
    }

    public virtual IQueryable<Question> GetQuery()
    {
        return _context.Questions
            .Include(q => q.QuestionAnswers)
            .Include(q => q.Publisher);
    }

    public virtual async Task UpdateAsync(Question entity)
    {
        _context.ChangeTracker.Clear();

        _context.Questions.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual IQueryable<Question> GetVisibleQuestionsQuery()
    {
        return _context.Questions
            .Where(q => q.Status == QuestionStatus.ACCEPTED)
            .Include(q => q.QuestionAnswers)
            .Include(q => q.Publisher)
            .AsQueryable();
    }

    public virtual async Task<Question[]> GetSuggestionsAsync()
    {
        return await _context.Questions
            .Where(q => q.Status != QuestionStatus.ACCEPTED)
            .Include(q => q.QuestionAnswers)
            .Include(q => q.Publisher)
            .ToArrayAsync();
    }

    public virtual async Task<Question?> GetByIdVisibleAsync(Guid id)
    {
        return await _context.Questions
            .Where(q => q.Status == QuestionStatus.ACCEPTED && q.Id == id)
            .Include(q => q.QuestionAnswers)
            .Include(q => q.Publisher)
            .FirstOrDefaultAsync();
    }

    public virtual async Task<Question?> GetSuggestionByIdAsync(Guid id)
    {
        var result = await GetByIdAsync(id);

        return result ?? (result!.Status == QuestionStatus.PENDING ? result : null);
    }

    public virtual async Task<IQueryable<Question>> GetFreeQuestionsAsync(IFreeQuestionsPoolService poolService)
    {
        var ids = poolService.GetQuestionsIds();

        return GetQuery()
            .Where(q => ids.Contains(q.Id));
    }

    public virtual async Task<bool> ExistsByTitleAsync(string title)
    {
        var loweredTitle = title.ToLower();

        return await _context.Questions
            .Where(q => q.Status != QuestionStatus.DECLINED && q.Title.ToLower() == loweredTitle)
            .AnyAsync();
    }

    public virtual async Task<IEnumerable<Question>> GetByTitleAsync(string title)
    {
        title = title.ToLower();

        return await _context.Questions
            .Where(q => q.Title.ToLower() == title)
            .Include(q => q.QuestionAnswers)
            .Include(q => q.Publisher)
            .ToArrayAsync();
    }

    public virtual async Task<int> GetQuestionsCountCached()
    {
        if (_fetchCount)
        {
            _countCache = await _context.Questions.CountAsync();
            _lastCountFetch = DateTime.Now;
        }

        return _countCache;
    }
}