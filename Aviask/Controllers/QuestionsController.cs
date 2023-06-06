using Aviask.Data;
using Aviask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Aviask.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly AviaskContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public QuestionsController(AviaskContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Questions
        public async Task<IActionResult> Index(MainCategoryType? category, SubCategoriesType? subcategory)
        {
            if (_context.Question == null)
            {
                Problem("Entity set 'AviaskContext.Question'  is null.");
                return NoContent();
            }

            var questionsQuery = _context.Question.Include(q => q.QuestionAnswers).AsQueryable();

            if (category != null)
            {
                questionsQuery = questionsQuery.Where(q => q.Category == category);
            }

            if (subcategory != null)
            {
                questionsQuery = questionsQuery.Where(q => q.SubCategory == subcategory);
            }

            return View(await questionsQuery.ToListAsync());
        }

        // GET: Questions/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Question == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var question = await _context.Question.Include(q => q.QuestionAnswers).FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated && question.Visibility != Visibility.Free)
            {
                return RedirectToPage("/Identity/Account/Register");
            }

            return View(question);
        }

        // GET: Questions/NextDetails/5 
        public async Task<IActionResult> NextDetails(int? id)
        {
            if (id == null || _context.Question == null)
            {
                id = 0;
            }

            var currentQuestion = await _context.Question.Include(q => q.QuestionAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);

            var nextQuestion = await _context.Question.OrderBy(q => Math.Abs(q.Id - currentQuestion.Id))
                .Include(q => q.QuestionAnswers)
                .FirstOrDefaultAsync(q => q.Id != currentQuestion.Id);

            if (nextQuestion == null || currentQuestion.Id >= nextQuestion.Id)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Details), nextQuestion);
        }

        [AllowAnonymous]
        public async Task<IActionResult> CheckAnswer(int? id, string? check)
        {
            if (id == null || _context.Question == null) return NotFound();

            if (check == null) return BadRequest();

            //  Retrieves the question based on given ID
            var question = await _context.Question.Include(q => q.QuestionAnswers).FirstOrDefaultAsync(m => m.Id == id);

            if (question == null) return NotFound();

            //  Redirect to register if the question isn't available to non-signed in users 
            if (question.Visibility != Visibility.Free && !User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Identity/Account/Register");
            }

            bool correctAnswer = question.QuestionAnswers.CorrectAnswer == check;

            //  If the user is logged in, add an answer record
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(HttpContext.User);

                _context.AnswerRecords.Add(new AnswerRecords
                {
                    UserId = userId,
                    QuestionId = question.Id,
                    Answered = check,
                    CorrectAnswer = correctAnswer
                });

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Id = id,
                IsCorrect = correctAnswer,
                CorrectAnswer = question.QuestionAnswers.CorrectAnswer,
                Explication = question.QuestionAnswers.Explications
            });
        }

        // GET: Questions/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Visibility,Category,SubCategory,QuestionAnswers")] Question question)
        {
            if (ModelState.IsValid)
            {
                if (question.QuestionAnswers.Answer3 == null)
                {
                    question.QuestionAnswers.NumberOfAnswers = 2;
                }
                else if (question.QuestionAnswers.Answer4 == null)
                {
                    question.QuestionAnswers.NumberOfAnswers = 3;
                }
                else
                {
                    question.QuestionAnswers.NumberOfAnswers = 4;
                }

                _context.QuestionAnswers.Add(question.QuestionAnswers);
                _context.Add(question.QuestionAnswers);
                _context.Add(question);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(question);
        }

        // GET: Questions/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Question == null)
            {
                return NotFound();
            }

            var question = await _context.Question.FindAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Visibility,Category,SubCategory,QuestionAnswers")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(question);
        }

        // GET: Questions/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Question == null)
            {
                return NotFound();
            }

            var question = await _context.Question.Include(q => q.QuestionAnswers).FirstOrDefaultAsync(q => q.Id == id);

            //var question = await _context.Question
            //    .FirstOrDefaultAsync(m => m.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Question == null)
            {
                return Problem("Entity set 'AviaskContext.Question'  is null.");
            }
            var question = await _context.Question.Include(q => q.QuestionAnswers).FirstOrDefaultAsync(q => q.Id == id);

            if (question != null)
            {
                _context.QuestionAnswers.Remove(question.QuestionAnswers);
                _context.Question.Remove(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return (_context.Question?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
