using Aviask.Data;
using Aviask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aviask.Controllers
{
    [Authorize(Roles = "admin")]
    public class AviaskUsersController : Controller
    {
        private readonly AviaskContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AviaskUsersController(AviaskContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var uvm = new UserViewModel
                {
                    User = user,
                    Roles = roles
                };

                userViewModels.Add(uvm);
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> DeleteUser(string? userid)
        {
            if (userid == null)
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);

            if (user == null)
            {
                return NotFound();
            }

            return View(new UserViewModel
            {
                User = user,
                Roles = await _userManager.GetRolesAsync(user),
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmedDeleteUser(string? userid)
        {
            if (userid == null)
            {
                return BadRequest();
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userid);

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }
    }
}
