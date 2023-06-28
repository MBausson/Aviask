using Aviask.Data;
using Aviask.Models;
using Microsoft.EntityFrameworkCore;

namespace Aviask.Repositories
{
    public class AnswerRecordsRepository : IAviaskRepository<AnswerRecords>
    {
        private readonly AviaskContext _context;

        public AnswerRecordsRepository(AviaskContext ctx) 
        { 
            _context = ctx;
        }

        public async Task<List<AnswerRecords>> GetRecordsFromUserIdAsync(string userId)
        {
            return await _context.AnswerRecords
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task CreateAsync(AnswerRecords obj)
        {
            await _context.AddAsync(obj);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AnswerRecords obj)
        {
            _context.Remove(obj);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteFromIdAsync(int id)
        {
            var obj = await GetByIdAsync(id);

            await DeleteAsync(obj);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _context.AnswerRecords.AnyAsync(x => x.Id == id);
        }

        public IQueryable<AnswerRecords> GetAll()
        {
            return _context.AnswerRecords.Include(r => r.Question);
        }

        public async Task<AnswerRecords?> GetByIdAsync(int id)
        {
            return await _context.AnswerRecords
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(AnswerRecords obj)
        {
            _context.Update(obj);

            await _context.SaveChangesAsync();
        }
    }
}
