using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aviask.Data;
using Aviask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Aviask.Controllers
{
    [Authorize]
    public class AnswerRecordsController : Controller
    {
        private readonly AviaskContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AnswerRecordsController(AviaskContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AnswerRecords
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var aviaskContext = _context.AnswerRecords.Include(a => a.Question).Where(r => r.UserId == userId);

            return View(await aviaskContext.ToListAsync());
        }
    }
}
