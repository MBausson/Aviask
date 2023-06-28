using Aviask.Data;
using Aviask.Migrations;
using Aviask.Models;
using Microsoft.EntityFrameworkCore;

namespace Aviask.Repositories
{
    public class QuestionRepository : IAviaskRepository<Question>
    {
        private readonly AviaskContext _context;

        public QuestionRepository(AviaskContext context)
        {
            _context = context;
        }

        public IQueryable<Question> GetAll()
        {
            return _context.Question
                .Include(q => q.QuestionAnswers)
                .Include(q => q.Publisher);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _context.Question.AnyAsync(x => x.Id == id);
        }

        public async Task CreateAsync(Question obj)
        {
            await _context.AddAsync(obj);
            await _context.QuestionAnswers.AddAsync(obj.QuestionAnswers);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Question obj)
        {
            if (obj.IllustrationPath != null)
            {
                string filePath = Path.Combine("wwwroot/", obj.IllustrationPath);
                File.Delete(filePath);
            }

            _context.Remove(obj.QuestionAnswers);
            _context.Remove(obj);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteFromIdAsync(int id)
        {
            var obj = await GetByIdAsync(id);

            if (obj == null)
            {
                return;
            }

            await DeleteAsync(obj);

            await _context.SaveChangesAsync();
        }

        public async Task<Question?> GetByIdAsync(int id)
        {
            return await (_context.Question
                .Include(x => x.QuestionAnswers)
                .Include(x => x.Publisher))
                .Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Question obj)
        {
            _context.Update(obj);

            await _context.SaveChangesAsync();
        }

        public async Task SaveIllustration(Question question, IFormFile file)
        {
            string savedFilePath = Path.Combine("wwwroot/", question.IllustrationPath!);

            using (var fileStream = new FileStream(savedFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }
    }
}
