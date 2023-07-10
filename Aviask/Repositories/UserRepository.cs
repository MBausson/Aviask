using Aviask.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aviask.Repositories
{
    public class UserRepository : IAviaskRepository<IdentityUser, string>
    {
        private readonly AviaskContext _context;

        public UserRepository(AviaskContext context)
        {
            _context = context;
        }

        public async Task<IdentityUser?> GetByUsername(string username)
        {
            return _context.Users
                .Where(u => u.UserName == username)
                .FirstOrDefault();
        }

        public async Task CreateAsync(IdentityUser obj)
        {
            _context.Add(obj);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(IdentityUser obj)
        {
            _context.Remove(obj);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteFromIdAsync(string id)
        {
            var obj = await GetByIdAsync(id);

            await DeleteAsync(obj);
        }

        public async Task<bool> ExistsByIdAsync(string id)
        {
            return GetByIdAsync(id) != null;
        }

        public IQueryable<IdentityUser> GetAll()
        {
            return _context.User.AsQueryable();
        }

        public async Task<IdentityUser?> GetByIdAsync(string id)
        {
            return await _context.User
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(IdentityUser obj)
        {
            _context.User.Update(obj);

            await _context.SaveChangesAsync();
        }
    }
}
