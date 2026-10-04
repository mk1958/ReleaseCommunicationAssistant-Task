using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReleaseCommunicationAssistant_Task.Models;

namespace ReleaseCommunicationAssistant_Task.Controllers
{
    public class ReviewDecisionsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewDecisionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ReviewDecisions
        public async Task<IActionResult> Index()
        {
            var reviewDecisions = await _context.ReviewDecisions
                .OrderByDescending(x => x.ReviewedAt)
                .ToListAsync();

            return View(reviewDecisions);
        }

        // GET: ReviewDecisions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var reviewDecision = await _context.ReviewDecisions
                .FirstOrDefaultAsync(x => x.ReviewDecisionId == id);

            if (reviewDecision == null)
                return NotFound();

            return View(reviewDecision);
        }

        // GET: ReviewDecisions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ReviewDecisions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ReviewDecisionId,GeneratedStatementId,Decision,EditedText,ReviewerName,ReviewComment")]
            ReviewDecision reviewDecision)
        {
            // Find the related GeneratedStatement
            var generatedStatement = await _context.GeneratedStatements
                .FirstOrDefaultAsync(x =>
                    x.GeneratedStatementId == reviewDecision.GeneratedStatementId);

            if (generatedStatement == null)
            {
                ModelState.AddModelError(
                    "GeneratedStatementId",
                    "The selected Generated Statement does not exist.");
            }
            else
            {
                // Assign the required navigation property
                reviewDecision.GeneratedStatement = generatedStatement;

                // Remove validation error for the navigation property
                // because it is loaded from the database above.
                ModelState.Remove("GeneratedStatement");
            }

            if (ModelState.IsValid)
            {
                _context.ReviewDecisions.Add(reviewDecision);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(reviewDecision);
        }

        // GET: ReviewDecisions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var reviewDecision = await _context.ReviewDecisions
                .FirstOrDefaultAsync(x => x.ReviewDecisionId == id);

            if (reviewDecision == null)
                return NotFound();

            return View(reviewDecision);
        }

        // POST: ReviewDecisions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ReviewDecisionId,GeneratedStatementId,Decision,EditedText,ReviewerName,ReviewComment")]
            ReviewDecision reviewDecision)
        {
            if (id != reviewDecision.ReviewDecisionId)
                return NotFound();

            // Load the related GeneratedStatement
            var generatedStatement = await _context.GeneratedStatements
                .FirstOrDefaultAsync(x =>
                    x.GeneratedStatementId == reviewDecision.GeneratedStatementId);

            if (generatedStatement == null)
            {
                ModelState.AddModelError(
                    "GeneratedStatementId",
                    "The selected Generated Statement does not exist.");
            }
            else
            {
                reviewDecision.GeneratedStatement = generatedStatement;

                // Remove navigation-property validation error
                ModelState.Remove("GeneratedStatement");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDecision = await _context.ReviewDecisions
                        .FirstOrDefaultAsync(x =>
                            x.ReviewDecisionId == id);

                    if (existingDecision == null)
                        return NotFound();

                    existingDecision.GeneratedStatementId =
                        reviewDecision.GeneratedStatementId;

                    existingDecision.Decision =
                        reviewDecision.Decision;

                    existingDecision.EditedText =
                        reviewDecision.EditedText;

                    existingDecision.ReviewerName =
                        reviewDecision.ReviewerName;

                    existingDecision.ReviewComment =
                        reviewDecision.ReviewComment;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewDecisionExists(
                        reviewDecision.ReviewDecisionId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(reviewDecision);
        }

        // GET: ReviewDecisions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var reviewDecision = await _context.ReviewDecisions
                .FirstOrDefaultAsync(x =>
                    x.ReviewDecisionId == id);

            if (reviewDecision == null)
                return NotFound();

            return View(reviewDecision);
        }

        // POST: ReviewDecisions/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reviewDecision = await _context.ReviewDecisions
                .FirstOrDefaultAsync(x =>
                    x.ReviewDecisionId == id);

            if (reviewDecision == null)
                return NotFound();

            _context.ReviewDecisions.Remove(reviewDecision);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ReviewDecisionExists(int id)
        {
            return _context.ReviewDecisions
                .Any(x => x.ReviewDecisionId == id);
        }
    }
}