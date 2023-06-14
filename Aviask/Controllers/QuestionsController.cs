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
        private static readonly int MaxPageLength = 15;

        public QuestionsController(AviaskContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Questions
        [AllowAnonymous]
        public async Task<IActionResult> Index(MainCategoryType? category, SubCategoriesType? subcategory, int page = 1)
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

            if (page < 1)
            {
                page = 1;
            }

            questionsQuery = questionsQuery.Skip((page - 1) * MaxPageLength)
                .Take(MaxPageLength);

            //  Non logged in users can only access question with visibility 'Free'
            if (!User.Identity.IsAuthenticated)
            {
                questionsQuery = questionsQuery.Where(q => q.Visibility == Visibility.Free);
            }

            ViewData["Page"] = page;

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
        [AllowAnonymous]
        public async Task<IActionResult> NextDetails(int? id)
        {
            if (id == null || _context.Question == null)
            {
                id = 0;
            }

            var currentQuestion = await _context.Question.Include(q => q.QuestionAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);

            IQueryable<Question> nextQuestionQuery = _context.Question;

            //  Non logged in user can only see free questions
            if (!User.Identity.IsAuthenticated)
            {
                nextQuestionQuery = nextQuestionQuery.Where(q => q.Visibility == Visibility.Free);
            }

            nextQuestionQuery = nextQuestionQuery
                .OrderBy(q => Math.Abs(q.Id - currentQuestion.Id))
                .Include(q => q.QuestionAnswers);

            var nextQuestion = await nextQuestionQuery.FirstOrDefaultAsync(q => q.Id != currentQuestion.Id);

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

            bool correctAnswer = question.QuestionAnswers.CorrectAnswer.Trim().ToLower() == check.Trim().ToLower();

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
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Visibility,Category,Source,SubCategory,QuestionAnswers")] Question question, IFormFile? illustrationFile)
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

                if (illustrationFile != null && illustrationFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(illustrationFile.FileName);
                    string filePath = Path.Combine("questions_images/") + fileName;
                    string savedFilePath = Path.Combine("wwwroot/", filePath);

                    await SaveIllustration(savedFilePath, illustrationFile);

                    question.IllustrationPath = filePath;
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

            var question = await _context.Question.Include(q => q.QuestionAnswers).FirstOrDefaultAsync(q => q.Id == id);

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Visibility,Category,Source,SubCategory,QuestionAnswers,QuestionAnswersId")] Question question, IFormFile? illustrationFile)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {   
                    //  Code boilerplate à changer
                    //  TODO
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

                    //  If the questions had an illustration, deletes it and add the new one.
                    if (illustrationFile != null)
                    {
                        if (question.IllustrationPath != null)
                        {
                            string oldFilePath = Path.Combine("wwwroot/questions_images", question.IllustrationPath);
                            System.IO.File.Delete(oldFilePath);
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(illustrationFile.FileName);
                        string filePath = Path.Combine("questions_images/", fileName);
                        string savedFilePath = Path.Combine("wwwroot/", filePath);

                        await SaveIllustration(savedFilePath, illustrationFile);
                        question.IllustrationPath = filePath;
                    }

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
                //  If the questions has an illustration, deletes it.
                if (question.IllustrationPath != null && question.IllustrationPath.Length > 0)
                {
                    string filePath = Path.Combine("wwwroot/", question.IllustrationPath);
                    System.IO.File.Delete(filePath);
                }

                _context.QuestionAnswers.Remove(question.QuestionAnswers);
                _context.Question.Remove(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        private bool QuestionExists(int id)
        {
            return (_context.Question?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private async Task SaveIllustration(string filePath, IFormFile file)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }

    }
}
