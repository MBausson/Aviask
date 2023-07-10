using Aviask.Data;
using Aviask.Models;
using Aviask.Repositories;
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
        private readonly IAviaskRepository<IdentityUser, string> _userRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public AviaskUsersController(AviaskContext context, UserManager<IdentityUser> userManager, IAviaskRepository<IdentityUser, string> userRepository)
        {
            _context = context;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAll().Take(50).ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var uvm = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
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

            var user = await _userRepository.GetByIdAsync(userid);

            if (user == null)
            {
                return NotFound();
            }

            return View(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                Roles = await _userManager.GetRolesAsync(user),
            });
        }

        public async Task<IActionResult> EditUser(string? userid)
        {
            if (userid == null)
            {
                return BadRequest();
            }

            var user = await _userRepository.GetByIdAsync(userid);

            if (user == null)
            {
                return NotFound();
            }

            return View(new UserViewModel()
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                Roles = await _userManager.GetRolesAsync(user),
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserViewModel editUserViewModel)
        {
            if (editUserViewModel == null)
            {
                return BadRequest();
            }

            var user = await _userRepository.GetByIdAsync(editUserViewModel.Id);
            user.UserName = editUserViewModel.UserName; //  Only the Username is editable for the moment

            await _userRepository.UpdateAsync(user);
            
            //  Updates user's roles
            foreach (var role in UserViewModel.AvailableRoles)
            {
                if (editUserViewModel.Roles.Contains(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                } else
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
            }

            return View(editUserViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmedDeleteUser(string? userid)
        {
            if (userid == null)
            {
                return BadRequest();
            }

            var user = await _userRepository.GetByIdAsync(userid);

            if (user == null)
            {
                return NotFound();
            }

            await _userRepository.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
