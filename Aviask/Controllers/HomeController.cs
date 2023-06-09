﻿using Aviask.Models;
using Aviask.Models.ResponseModels;
using Aviask.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Aviask.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAviaskRepository<AnswerRecords, int> _answerRecordsRepository;
        private readonly IAviaskRepository<Question, int> _questionRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(IAviaskRepository<AnswerRecords, int> recordsRepository, IAviaskRepository<Question, int> questionRepository,UserManager<IdentityUser> userManager)
        {
            _answerRecordsRepository = recordsRepository;
            _questionRepository = questionRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            //  We pass the count of registered questions.
            ViewData["QuestionCount"] = await _questionRepository.GetAll().CountAsync();

            //  If the user is authenticated, we also pass its statistics.
            if (User.Identity.IsAuthenticated)
            {
                ViewData["Statistics"] = await _getStatisticsAsync(_userManager.GetUserId(User)!);
            }

            return View();
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

        [NonAction]
        private async Task<UserStatisticsResponse> _getStatisticsAsync(string userId)
        {
            var userRecordsQuery = _answerRecordsRepository.GetAll();
            userRecordsQuery = userRecordsQuery.Where(r => r.UserId == userId);

            //  Counts how much questions were answered since last 7 days
            var now = DateTime.Now;
            var lastWeekRecords = (await userRecordsQuery.ToListAsync()).Where(r => (now - r.Date).TotalDays < 7).Count();

            //  Calculates lifetime answer correctness
            int correctLifetime = await userRecordsQuery.Where(r => r.CorrectAnswer).CountAsync();
            int failLifetime = await userRecordsQuery.Where(r => !r.CorrectAnswer).CountAsync();
            float ratio;

            if (failLifetime + correctLifetime != 0)
            {
                ratio = (float)correctLifetime / (failLifetime + correctLifetime);
            }
            else
            {
                ratio = 0;
            }

            //  Computes the 3 most answered subcategories
            var mostAnsweredThemes = userRecordsQuery
                .AsEnumerable()
                .GroupBy(r => r.Question.SubCategory)
                .Select(g => new StatisticsCategoryResponse(g.Key, g.Key.ToString(), g.Count()))
                .OrderByDescending(g => g.AnswerCount)
                .Take(3)
                .ToList();

            //  Computes the most correctly answered category
            var mostCorrectlyTheme = userRecordsQuery
                .AsEnumerable()
                .Where(r => r.CorrectAnswer)
                .GroupBy(r => r.Question.SubCategory)
                .Select(g => new StatisticsCategoryResponse(g.Key, g.Key.ToString(), g.Count()))
                .OrderByDescending(g => g.AnswerCount)
                .FirstOrDefault();

            return new UserStatisticsResponse(
                lastWeekRecords,
                correctLifetime,
                failLifetime,
                ratio,
                mostCorrectlyTheme!,
                mostAnsweredThemes
            );
        }
    }
}