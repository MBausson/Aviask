using Aviask.Data;
using Aviask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aviask.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AviaskContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, AviaskContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            //  We pass as viewdata the count of registered questions.
            ViewData["QuestionCount"] = await _context.Question.CountAsync();

            return View();
        }

        [Authorize]
        public async Task<IActionResult> UserStatistics()
        {
            Debug.WriteLine("zz");
            var id = _userManager.GetUserId(User);

            var userRecordsQuery = from r in _context.AnswerRecords where r.UserId == id select r;

            //  Counts how much questions were answered since last 7 days
            var now = DateTime.Now;
            var lastWeekRecords = (await userRecordsQuery.ToListAsync()).Where(r => (now - r.Date).TotalDays < 7).Count();

            //  Calculates lifetime answer correctness
            int correctLifetime = await userRecordsQuery.Where(r => r.CorrectAnswer).CountAsync();
            int failLifetime = await userRecordsQuery.Where(r => !r.CorrectAnswer).CountAsync();
            float ratio;
           
            if (failLifetime != 0)
            {
                ratio = (float)correctLifetime / (failLifetime + correctLifetime);
            } else
            {
                ratio = 0;
            }

            //  Computes the 3 most answered subcategories
            var mostAnsweredThemes = (await userRecordsQuery
                .GroupBy(r => r.Question.SubCategory)
                .Select(g => new
                {
                    SubCategory = g.Key,
                    SubCategoryName = g.Key.ToString(),
                    AnswerCount = g.Count()
                })
                .OrderByDescending(g => g.AnswerCount)
                .Take(3)
                .ToListAsync());

            //  Computes the most correctly answered category
            var mostCorrectlyTheme = await userRecordsQuery
                .Where(r => r.CorrectAnswer)
                .GroupBy(r => r.Question.SubCategory)
                .Select(g => new
                {
                    SubCategory = g.Key,
                    SubCategoryName = g.Key.ToString(),
                    CorrectAnswerCount = g.Count()
                })
                .OrderByDescending(g => g.CorrectAnswerCount)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                LastWeekRecordsCount = lastWeekRecords,
                CorrectLifetime = correctLifetime,
                FailLifetime = failLifetime,
                RatioCorrectness = Math.Round(ratio, 2),
                FavoriteThemes = mostAnsweredThemes,    
                MostCorrectlyTheme = mostCorrectlyTheme
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}