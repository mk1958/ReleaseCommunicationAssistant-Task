using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReleaseCommunicationAssistant_Task.Models;

namespace ReleaseCommunicationAssistant_Task.Controllers
{
    public class GeneratedStatementsController : Controller
    {
        private readonly AppDbContext _context;

        public GeneratedStatementsController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // MARK STATEMENT AS STALE
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkStale(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var statement = await _context.GeneratedStatements
                .FirstOrDefaultAsync(x =>
                    x.GeneratedStatementId == id);

            if (statement == null)
            {
                return NotFound();
            }

            statement.IsStale = true;

            statement.StaleReason =
                "The release package was changed after this statement was generated. Review the statement again.";

            if (string.IsNullOrWhiteSpace(statement.Status))
            {
                statement.Status = "Draft";
            }

            statement.UpdatedAt = DateTime.UtcNow;

            _context.Entry(statement).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Statement {statement.StatementCode} was marked as stale.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: GeneratedStatements
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var statements = await _context.GeneratedStatements
                .Include(x => x.Release)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(statements);
        }


        // =========================================================
        // GET: GeneratedStatements/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generatedStatement = await _context.GeneratedStatements
                .Include(x => x.Release)
                .FirstOrDefaultAsync(x =>
                    x.GeneratedStatementId == id);

            if (generatedStatement == null)
            {
                return NotFound();
            }

            return View(generatedStatement);
        }


        // =========================================================
        // GET: GeneratedStatements/Create
        // =========================================================
        public async Task<IActionResult> Create()
        {
            ViewBag.Releases = await _context.Releases
                .OrderByDescending(x => x.ReleaseId)
                .ToListAsync();

            return View();
        }


        // =========================================================
        // POST: GeneratedStatements/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("GeneratedStatementId,ReleaseId,StatementCode,Audience,StatementType,StatementText,ImpactLevel,EvidenceReferences,IsSupported,IsStale,StaleReason,Status,Aimodel")]
            GeneratedStatements generatedStatement)
        {
            var releaseExists = await _context.Releases
                .AnyAsync(x =>
                    x.ReleaseId == generatedStatement.ReleaseId);

            if (!releaseExists)
            {
                ModelState.AddModelError(
                    "ReleaseId",
                    "The selected release does not exist.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Releases = await _context.Releases
                    .OrderByDescending(x => x.ReleaseId)
                    .ToListAsync();

                return View(generatedStatement);
            }

            generatedStatement.CreatedAt = DateTime.UtcNow;
            generatedStatement.UpdatedAt = DateTime.UtcNow;

            _context.GeneratedStatements.Add(generatedStatement);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: GeneratedStatements/Edit/5
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generatedStatement =
                await _context.GeneratedStatements.FindAsync(id);

            if (generatedStatement == null)
            {
                return NotFound();
            }

            return View(generatedStatement);
        }


        // =========================================================
        // POST: GeneratedStatements/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("GeneratedStatementId,ReleaseId,StatementCode,Audience,StatementType,StatementText,ImpactLevel,EvidenceReferences,IsSupported,IsStale,StaleReason,Status,Aimodel")]
            GeneratedStatements generatedStatement)
        {
            if (id != generatedStatement.GeneratedStatementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(generatedStatement);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneratedStatementExists(
                        generatedStatement.GeneratedStatementId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(generatedStatement);
        }


        // =========================================================
        // REVIEW GENERATED STATEMENT
        // =========================================================

        // GET: GeneratedStatements/Review/5
        public async Task<IActionResult> Review(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var statement = await _context.GeneratedStatements
                .Include(x => x.Release)
                .FirstOrDefaultAsync(x =>
                    x.GeneratedStatementId == id);

            if (statement == null)
            {
                return NotFound();
            }

            var model = new ReviewStatementViewModel
            {
                GeneratedStatementId =
                    statement.GeneratedStatementId,

                ReleaseId =
                    statement.ReleaseId,

                VersionNumber =
                    statement.Release?.VersionNumber ?? "",

                Audience =
                    statement.Audience ?? "",

                StatementType =
                    statement.StatementType ?? "",

                StatementText =
                    statement.StatementText ?? "",

                ImpactLevel =
                    statement.ImpactLevel ?? "",

                EvidenceReferences =
                    statement.EvidenceReferences ?? "",

                IsSupported =
                    statement.IsSupported,

                IsStale =
                    statement.IsStale,

                StaleReason =
                    statement.StaleReason,

                Status =
                    statement.Status ?? "Draft"
            };

            return View(model);
        }


        // =========================================================
        // POST: GeneratedStatements/Review/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(
            ReviewStatementViewModel model,
            string decision)
        {
            var statement = await _context.GeneratedStatements
                .FirstOrDefaultAsync(x =>
                    x.GeneratedStatementId ==
                    model.GeneratedStatementId);

            if (statement == null)
            {
                return NotFound();
            }

            // Normalize the decision to match the database
            // CHECK constraint values.
            var normalizedDecision =
                decision.Equals(
                    "Approve",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Approved"
                    : decision.Equals(
                        "Reject",
                        StringComparison.OrdinalIgnoreCase)
                        ? "Rejected"
                        : "Edited";

            var finalText =
                string.IsNullOrWhiteSpace(model.EditedText)
                    ? statement.StatementText
                    : model.EditedText;

            var reviewDecision = new ReviewDecision
            {
                GeneratedStatementId =
                    statement.GeneratedStatementId,

                Decision =
                    normalizedDecision,

                EditedText =
                    model.EditedText,

                ReviewerName =
                    model.ReviewerName,

                ReviewComment =
                    model.ReviewComment,

                ReviewedAt =
                    DateTime.UtcNow
            };

            _context.ReviewDecisions.Add(reviewDecision);


            // =====================================================
            // APPROVE
            // =====================================================
            if (decision.Equals(
                "Approve",
                StringComparison.OrdinalIgnoreCase))
            {
                statement.Status = "Approved";

                statement.StatementText = finalText;

                statement.IsStale = false;

                statement.StaleReason = null;
            }


            // =====================================================
            // REJECT
            // =====================================================
            else if (decision.Equals(
                "Reject",
                StringComparison.OrdinalIgnoreCase))
            {
                statement.Status = "Rejected";
            }


            // =====================================================
            // EDIT
            // =====================================================
            else
            {
                statement.Status = "Edited";

                statement.StatementText = finalText;
            }

            statement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: GeneratedStatements/Delete/5
        // =========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generatedStatement =
                await _context.GeneratedStatements
                    .FirstOrDefaultAsync(x =>
                        x.GeneratedStatementId == id);

            if (generatedStatement == null)
            {
                return NotFound();
            }

            return View(generatedStatement);
        }


        // =========================================================
        // POST: GeneratedStatements/Delete/5
        // =========================================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var generatedStatement =
                await _context.GeneratedStatements.FindAsync(id);

            if (generatedStatement != null)
            {
                _context.GeneratedStatements.Remove(
                    generatedStatement);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // CHECK STATEMENT EXISTS
        // =========================================================
        private bool GeneratedStatementExists(int id)
        {
            return _context.GeneratedStatements
                .Any(x => x.GeneratedStatementId == id);
        }


        // ==========================================
        // QA CLAIM SUPPORT ANALYSIS
        // ==========================================
        public async Task<IActionResult> QASupport(int? id)
        {
            if (id == null)
                return NotFound();

            var statement = await _context.GeneratedStatements
                .Include(x => x.Release)
                .FirstOrDefaultAsync(x =>
                    x.GeneratedStatementId == id);

            if (statement == null)
                return NotFound();

            var qaEvidence = await _context.Qaevidences
                .Where(x => x.ReleaseId == statement.ReleaseId)
                .ToListAsync();

            var model = new QASupportViewModel
            {
                GeneratedStatementId =
                    statement.GeneratedStatementId,

                ReleaseId =
                    statement.ReleaseId,

                VersionNumber =
                    statement.Release?.VersionNumber ?? "",

                StatementCode =
                    statement.StatementCode,

                StatementText =
                    statement.StatementText,

                EvidenceReferences =
                    statement.EvidenceReferences ?? ""
            };

            var referencedCodes =
                (statement.EvidenceReferences ?? "")
                .Split(
                    new[] { ',', ';', ' ' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var supportingEvidence = qaEvidence
                .Where(x =>
                    referencedCodes.Any(code =>
                        string.Equals(
                            code,
                            x.EvidenceCode,
                            StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var evidence in supportingEvidence)
            {
                model.SupportingEvidence.Add(
                    new QASupportEvidence
                    {
                        EvidenceCode =
                            evidence.EvidenceCode ?? "",

                        TestArea =
                            evidence.TestArea ?? "",

                        Result =
                            evidence.Result ?? "",

                        EvidenceDescription =
                            evidence.EvidenceDescription ?? ""
                    });
            }

            if (!referencedCodes.Any())
            {
                model.IsSupported = false;

                model.SupportStatus =
                    "Unsupported";

                model.AnalysisReason =
                    "The statement does not reference any supplied QA evidence.";

                model.UnsupportedClaims.Add(
                    "The statement has no QA evidence reference supporting its claim.");
            }
            else if (!supportingEvidence.Any())
            {
                model.IsSupported = false;

                model.SupportStatus =
                    "Unsupported";

                model.AnalysisReason =
                    "The statement references evidence codes that do not match the supplied QA evidence.";

                model.UnsupportedClaims.Add(
                    "No supplied QA evidence matches the evidence references in this statement.");
            }
            else if (supportingEvidence.Any(x =>
                string.Equals(
                    x.Result,
                    "Pass",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    x.Result,
                    "Passed",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    x.Result,
                    "Success",
                    StringComparison.OrdinalIgnoreCase)))
            {
                model.IsSupported = true;

                model.SupportStatus =
                    "Supported";

                model.AnalysisReason =
                    "The statement references supplied QA evidence with a successful test result.";
            }
            else
            {
                model.IsSupported = false;

                model.SupportStatus =
                    "Unsupported";

                model.AnalysisReason =
                    "The referenced QA evidence does not contain a successful test result.";

                model.UnsupportedClaims.Add(
                    "The supplied QA evidence does not demonstrate a successful test result for this statement.");
            }

            return View(model);
        }
    }
}