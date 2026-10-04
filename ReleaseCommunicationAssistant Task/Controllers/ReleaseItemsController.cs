using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReleaseCommunicationAssistant_Task.Models;

namespace ReleaseCommunicationAssistant_Task.Controllers
{
    public class ReleaseItemsController : Controller
    {
        private readonly AppDbContext _context;

        public ReleaseItemsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var releaseItems = await _context.ReleaseItems
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(releaseItems);
        }

        // ==========================================
        // AUTOMATIC USER IMPACT CLASSIFICATION
        // ==========================================
        public async Task<IActionResult> Impact(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.ReleaseItems
                .FirstOrDefaultAsync(x => x.ReleaseItemId == id);

            if (item == null)
                return NotFound();

            var impactLevel = ClassifyImpact(
                item.ItemType,
                item.UserImpact,
                item.ImpactReason,
                item.AffectedUsers,
                item.Description);

            var model = new ReleaseImpactViewModel
            {
                ReleaseItemId = item.ReleaseItemId,
                EvidenceCode = item.EvidenceCode ?? string.Empty,
                ItemType = item.ItemType ?? string.Empty,
                Title = item.Title ?? string.Empty,
                UserImpact = item.UserImpact ?? string.Empty,
                ImpactReason = item.ImpactReason ?? string.Empty,
                AffectedUsers = item.AffectedUsers ?? string.Empty,
                ImpactLevel = impactLevel
            };

            return View(model);
        }

        private string ClassifyImpact(
            string? itemType,
            string? userImpact,
            string? impactReason,
            string? affectedUsers,
            string? description)
        {
            var text = string.Join(" ",
                itemType ?? "",
                userImpact ?? "",
                impactReason ?? "",
                affectedUsers ?? "",
                description ?? "")
                .ToLower();

            // HIGH IMPACT
            if (text.Contains("all users") ||
                text.Contains("all customers") ||
                text.Contains("security") ||
                text.Contains("authentication") ||
                text.Contains("payment") ||
                text.Contains("data loss") ||
                text.Contains("breaking change") ||
                text.Contains("critical"))
            {
                return "High";
            }

            // MEDIUM IMPACT
            if (text.Contains("user") ||
                text.Contains("customer") ||
                text.Contains("workflow") ||
                text.Contains("performance") ||
                text.Contains("configuration") ||
                text.Contains("migration") ||
                text.Contains("api") ||
                text.Contains("integration") ||
                text.Contains("moderate"))
            {
                return "Medium";
            }

            // LOW IMPACT
            return "Low";
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var releaseItem = await _context.ReleaseItems
                .FirstOrDefaultAsync(x => x.ReleaseItemId == id);

            if (releaseItem == null)
                return NotFound();

            return View(releaseItem);
        }

        // ==========================================
        // CREATE RELEASE ITEM
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ReleaseItemId,ReleaseId,EvidenceCode,ItemType,Title,Description,UserImpact,ImpactReason,AffectedUsers")]
            ReleaseItem releaseItem)
        {
            if (ModelState.IsValid)
            {
                _context.ReleaseItems.Add(releaseItem);

                // ==========================================
                // MARK GENERATED STATEMENTS AS STALE
                // ==========================================
                var generatedStatements = await _context.GeneratedStatements
                    .Where(x =>
                        x.ReleaseId == releaseItem.ReleaseId &&
                        !x.IsStale)
                    .ToListAsync();

                foreach (var statement in generatedStatements)
                {
                    statement.IsStale = true;

                    statement.StaleReason =
                        "A release item was added after this statement was generated. Review the statement again.";

                    if (string.IsNullOrWhiteSpace(statement.Status))
                    {
                        statement.Status = "Draft";
                    }

                    statement.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(releaseItem);
        }

        // ==========================================
        // EDIT RELEASE ITEM
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var releaseItem = await _context.ReleaseItems
                .FirstOrDefaultAsync(x => x.ReleaseItemId == id);

            if (releaseItem == null)
                return NotFound();

            return View(releaseItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ReleaseItemId,ReleaseId,EvidenceCode,ItemType,Title,Description,UserImpact,ImpactReason,AffectedUsers")]
            ReleaseItem releaseItem)
        {
            if (id != releaseItem.ReleaseItemId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _context.ReleaseItems
                        .FirstOrDefaultAsync(x => x.ReleaseItemId == id);

                    if (existingItem == null)
                        return NotFound();

                    var originalReleaseId = existingItem.ReleaseId;

                    existingItem.ReleaseId = releaseItem.ReleaseId;
                    existingItem.EvidenceCode = releaseItem.EvidenceCode;
                    existingItem.ItemType = releaseItem.ItemType;
                    existingItem.Title = releaseItem.Title;
                    existingItem.Description = releaseItem.Description;
                    existingItem.UserImpact = releaseItem.UserImpact;
                    existingItem.ImpactReason = releaseItem.ImpactReason;
                    existingItem.AffectedUsers = releaseItem.AffectedUsers;

                    // ==========================================
                    // MARK GENERATED STATEMENTS AS STALE
                    // ==========================================
                    var releaseIds = new List<int>
                    {
                        originalReleaseId,
                        releaseItem.ReleaseId
                    };

                    var generatedStatements = await _context.GeneratedStatements
                        .Where(x =>
                            releaseIds.Contains(x.ReleaseId) &&
                            !x.IsStale)
                        .ToListAsync();

                    foreach (var statement in generatedStatements)
                    {
                        statement.IsStale = true;

                        statement.StaleReason =
                            "A release item was changed after this statement was generated. Review the statement again.";

                        if (string.IsNullOrWhiteSpace(statement.Status))
                        {
                            statement.Status = "Draft";
                        }

                        statement.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReleaseItemExists(releaseItem.ReleaseItemId))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(releaseItem);
        }

        // ==========================================
        // DELETE RELEASE ITEM
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var releaseItem = await _context.ReleaseItems
                .FirstOrDefaultAsync(x => x.ReleaseItemId == id);

            if (releaseItem == null)
                return NotFound();

            return View(releaseItem);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var releaseItem = await _context.ReleaseItems
                .FirstOrDefaultAsync(x => x.ReleaseItemId == id);

            if (releaseItem == null)
                return NotFound();

            var releaseId = releaseItem.ReleaseId;

            _context.ReleaseItems.Remove(releaseItem);

            // ==========================================
            // MARK GENERATED STATEMENTS AS STALE
            // ==========================================
            var generatedStatements = await _context.GeneratedStatements
                .Where(x =>
                    x.ReleaseId == releaseId &&
                    !x.IsStale)
                .ToListAsync();

            foreach (var statement in generatedStatements)
            {
                statement.IsStale = true;

                statement.StaleReason =
                    "A release item was deleted after this statement was generated. Review the statement again.";

                if (string.IsNullOrWhiteSpace(statement.Status))
                {
                    statement.Status = "Draft";
                }

                statement.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ReleaseItemExists(int id)
        {
            return _context.ReleaseItems.Any(x => x.ReleaseItemId == id);
        }
    }
}