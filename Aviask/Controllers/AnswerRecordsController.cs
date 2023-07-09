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
using Aviask.Repositories;

namespace Aviask.Controllers
{
    [Authorize]
    public class AnswerRecordsController : Controller
    {
        private readonly IAviaskRepository<AnswerRecords, int> _answerRecordsRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public AnswerRecordsController(IAviaskRepository<AnswerRecords, int> answerRecordsRepository, UserManager<IdentityUser> userManager)
        {
            _answerRecordsRepository = answerRecordsRepository;
            _userManager = userManager;
        }

        // GET: AnswerRecords
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var records = await ((AnswerRecordsRepository)_answerRecordsRepository).GetRecordsFromUserIdAsync(userId);

            return View(records);
        }
    }
}
