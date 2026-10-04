using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReleaseCommunicationAssistant_Task.Models;

namespace ReleaseCommunicationAssistant_Task.Controllers
{
    public class QaevidencesController : Controller
    {
        private readonly AppDbContext _context;

        public QaevidencesController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // INDEX
        // ==========================================
        public async Task<IActionResult> Index()
        {
            return View(await _context.Qaevidences.ToListAsync());
        }

        // ==========================================
        // QA CLAIM SUPPORT CHECK
        // ==========================================
        public async Task<IActionResult> ClaimCheck(int? id)
        {
            if (id == null)
                return NotFound();

            var releaseItem = await _context.ReleaseItems
                .FirstOrDefaultAsync(x => x.ReleaseItemId == id);

            if (releaseItem == null)
                return NotFound();

            var claimText = string.Join(" ",
                releaseItem.Title ?? "",
                releaseItem.Description ?? "",
                releaseItem.UserImpact ?? "",
                releaseItem.ImpactReason ?? "")
                .Trim();

            var qaEvidence = await _context.Qaevidences
                .Where(x => x.ReleaseId == releaseItem.ReleaseId)
                .ToListAsync();

            Qaevidence? matchedEvidence = null;

            foreach (var evidence in qaEvidence)
            {
                var evidenceText = string.Join(" ",
                    evidence.EvidenceCode ?? "",
                    evidence.TestArea ?? "",
                    evidence.EvidenceDescription ?? "",
                    evidence.Result ?? "")
                    .ToLower();

                var claimWords = claimText
                    .ToLower()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => x.Length >= 4)
                    .Distinct()
                    .ToList();

                var matchingWords = claimWords
                    .Where(word => evidenceText.Contains(word))
                    .ToList();

                if (matchingWords.Count >= 2)
                {
                    matchedEvidence = evidence;
                    break;
                }
            }

            var model = new QaClaimCheckViewModel
            {
                ReleaseItemId = releaseItem.ReleaseItemId,
                ReleaseId = releaseItem.ReleaseId,
                EvidenceCode = releaseItem.EvidenceCode ?? "",
                Claim = claimText
            };

            if (matchedEvidence != null)
            {
                model.IsSupported = true;
                model.Result = "Supported";
                model.EvidenceDescription =
                    matchedEvidence.EvidenceDescription ?? "";
                model.TestArea =
                    matchedEvidence.TestArea ?? "";

                model.SupportReason =
                    $"The claim has matching QA evidence ({matchedEvidence.EvidenceCode}).";
            }
            else
            {
                model.IsSupported = false;
                model.Result = "Unsupported";
                model.SupportReason =
                    "No supplied QA evidence sufficiently supports this release claim.";
            }

            return View(model);
        }

        // ==========================================
        // DETAILS
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var qaevidence = await _context.Qaevidences
                .FirstOrDefaultAsync(x => x.QaevidenceId == id);

            if (qaevidence == null)
                return NotFound();

            return View(qaevidence);
        }

        // ==========================================
        // CREATE
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("QaevidenceId,ReleaseId,EvidenceCode,TestArea,EvidenceDescription,Result,TestDate")]
            Qaevidence qaevidence)
        {
            if (ModelState.IsValid)
            {
                _context.Qaevidences.Add(qaevidence);

                var generatedStatements =
                    await _context.GeneratedStatements
                        .Where(x =>
                            x.ReleaseId == qaevidence.ReleaseId)
                        .ToListAsync();

                foreach (var statement in generatedStatements)
                {
                    statement.IsStale = true;

                    statement.StaleReason =
                        "QA evidence was added after this statement was generated. Review the statement again.";

                    statement.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(qaevidence);
        }

        // ==========================================
        // EDIT
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var qaevidence = await _context.Qaevidences
                .FindAsync(id);

            if (qaevidence == null)
                return NotFound();

            return View(qaevidence);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("QaevidenceId,ReleaseId,EvidenceCode,TestArea,EvidenceDescription,Result,TestDate")]
            Qaevidence qaevidence)
        {
            if (id != qaevidence.QaevidenceId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(qaevidence);

            try
            {
                var existingEvidence =
                    await _context.Qaevidences
                        .FirstOrDefaultAsync(x =>
                            x.QaevidenceId == id);

                if (existingEvidence == null)
                    return NotFound();

                var originalReleaseId =
                    existingEvidence.ReleaseId;

                // Update QA evidence
                existingEvidence.ReleaseId =
                    qaevidence.ReleaseId;

                existingEvidence.EvidenceCode =
                    qaevidence.EvidenceCode;

                existingEvidence.TestArea =
                    qaevidence.TestArea;

                existingEvidence.EvidenceDescription =
                    qaevidence.EvidenceDescription;

                existingEvidence.Result =
                    qaevidence.Result;

                existingEvidence.TestDate =
                    qaevidence.TestDate;

                // ==========================================
                // MARK RELATED STATEMENTS AS STALE
                // ==========================================

                var affectedReleaseIds =
                    new List<int>
                    {
                        originalReleaseId,
                        qaevidence.ReleaseId
                    }
                    .Distinct()
                    .ToList();

                var generatedStatements =
                    await _context.GeneratedStatements
                        .Where(x =>
                            affectedReleaseIds.Contains(x.ReleaseId))
                        .ToListAsync();

                foreach (var statement in generatedStatements)
                {
                    statement.IsStale = true;

                    statement.StaleReason =
                        "QA evidence was changed after this statement was generated. Review the statement again.";

                    statement.UpdatedAt =
                        DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QaevidenceExists(qaevidence.QaevidenceId))
                    return NotFound();

                throw;
            }
        }

        // ==========================================
        // DELETE
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var qaevidence = await _context.Qaevidences
                .FirstOrDefaultAsync(x => x.QaevidenceId == id);

            if (qaevidence == null)
                return NotFound();

            return View(qaevidence);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var qaevidence =
                await _context.Qaevidences.FindAsync(id);

            if (qaevidence == null)
                return NotFound();

            var releaseId =
                qaevidence.ReleaseId;

            _context.Qaevidences.Remove(qaevidence);

            // ==========================================
            // MARK RELATED STATEMENTS AS STALE
            // ==========================================

            var generatedStatements =
                await _context.GeneratedStatements
                    .Where(x =>
                        x.ReleaseId == releaseId)
                    .ToListAsync();

            foreach (var statement in generatedStatements)
            {
                statement.IsStale = true;

                statement.StaleReason =
                    "QA evidence was deleted after this statement was generated. Review the statement again.";

                statement.UpdatedAt =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool QaevidenceExists(int id)
        {
            return _context.Qaevidences
                .Any(x => x.QaevidenceId == id);
        }
    }
}