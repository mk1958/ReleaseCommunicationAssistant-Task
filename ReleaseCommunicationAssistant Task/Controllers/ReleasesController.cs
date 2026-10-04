using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReleaseCommunicationAssistant_Task.Models;
using ReleaseCommunicationAssistant_Task.Services;

namespace ReleaseCommunicationAssistant_Task.Controllers
{
    public class ReleasesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly OpenAIReleaseAnalysisService _openAIService;

        public ReleasesController(
            AppDbContext context,
            OpenAIReleaseAnalysisService openAIService)
        {
            _context = context;
            _openAIService = openAIService;
        }

        // ==========================================
        // RELEASE LIST
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var releases = await _context.Releases
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(releases);
        }

        // ==========================================
        // RELEASE DETAILS
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            return View(release);
        }

        // ==========================================
        // RELEASE READINESS CHECK
        // ==========================================
        public async Task<IActionResult> Readiness(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            var items = await _context.ReleaseItems
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var qaEvidence = await _context.Qaevidences
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var result = new ReleaseReadinessViewModel
            {
                ReleaseId = release.ReleaseId,
                VersionNumber = release.VersionNumber,
                Status = release.Status,

                HasCompletedFeatures = items.Any(x =>
                    x.ItemType != null &&
                    x.ItemType.ToLower().Contains("feature")),

                HasBugFixes = items.Any(x =>
                    x.ItemType != null &&
                    (x.ItemType.ToLower().Contains("bug") ||
                     x.ItemType.ToLower().Contains("fix"))),

                HasChangedBehaviour = items.Any(x =>
                    x.ItemType != null &&
                    (x.ItemType.ToLower().Contains("behavior") ||
                     x.ItemType.ToLower().Contains("behaviour") ||
                     x.ItemType.ToLower().Contains("change"))),

                HasQaSummary = qaEvidence.Any(),

                HasKnownLimitations =
                    !string.IsNullOrWhiteSpace(release.KnownLimitations),

                HasMigrationNotes = items.Any(x =>
                    !string.IsNullOrWhiteSpace(x.Description) &&
                    (x.Description.ToLower().Contains("migration") ||
                     x.Description.ToLower().Contains("configuration") ||
                     x.Description.ToLower().Contains("config"))),

                HasAffectedUserGroups = items.Any(x =>
                    !string.IsNullOrWhiteSpace(x.AffectedUsers))
            };

            if (!result.HasCompletedFeatures)
                result.MissingSections.Add("Completed Features");

            if (!result.HasBugFixes)
                result.MissingSections.Add("Bug Fixes");

            if (!result.HasChangedBehaviour)
                result.MissingSections.Add("Changed Behaviour");

            if (!result.HasQaSummary)
                result.MissingSections.Add("QA Summary");

            if (!result.HasKnownLimitations)
                result.MissingSections.Add("Known Limitations");

            if (!result.HasMigrationNotes)
                result.MissingSections.Add(
                    "Migration / Configuration Notes");

            if (!result.HasAffectedUserGroups)
                result.MissingSections.Add("Affected User Groups");

            result.IsComplete =
                result.MissingSections.Count == 0;

            return View(result);
        }

        // ==========================================
        // RELEASE RISK & LIMITATION IDENTIFICATION
        // ==========================================
        public async Task<IActionResult> Risks(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            var items = await _context.ReleaseItems
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var model = new ReleaseRiskViewModel
            {
                ReleaseId = release.ReleaseId,
                VersionNumber = release.VersionNumber
            };

            foreach (var item in items)
            {
                var text = string.Join(" ",
                    item.Title ?? "",
                    item.Description ?? "",
                    item.UserImpact ?? "",
                    item.ImpactReason ?? "",
                    item.AffectedUsers ?? "")
                    .ToLower();

                // HIGH RISK
                if (text.Contains("security") ||
                    text.Contains("authentication") ||
                    text.Contains("authorization") ||
                    text.Contains("data loss") ||
                    text.Contains("breaking change") ||
                    text.Contains("critical"))
                {
                    model.Risks.Add(new ReleaseRiskItem
                    {
                        EvidenceCode = item.EvidenceCode ?? "",
                        Title = item.Title ?? "",
                        Description = item.Description ?? "",
                        Reason =
                            "The release item contains a high-risk change indicator.",
                        Severity = "High"
                    });
                }

                // MEDIUM RISK
                else if (text.Contains("performance") ||
                         text.Contains("migration") ||
                         text.Contains("configuration") ||
                         text.Contains("api") ||
                         text.Contains("integration") ||
                         text.Contains("workflow"))
                {
                    model.Risks.Add(new ReleaseRiskItem
                    {
                        EvidenceCode = item.EvidenceCode ?? "",
                        Title = item.Title ?? "",
                        Description = item.Description ?? "",
                        Reason =
                            "The release item may affect existing workflows, integrations, or configuration.",
                        Severity = "Medium"
                    });
                }

                // LIMITATIONS
                if (text.Contains("limitation") ||
                    text.Contains("known issue") ||
                    text.Contains("not supported") ||
                    text.Contains("unsupported") ||
                    text.Contains("workaround") ||
                    text.Contains("manual"))
                {
                    model.Limitations.Add(new ReleaseRiskItem
                    {
                        EvidenceCode = item.EvidenceCode ?? "",
                        Title = item.Title ?? "",
                        Description = item.Description ?? "",
                        Reason =
                            "The release item contains a known limitation or operational constraint.",
                        Severity = "Attention"
                    });
                }
            }

            if (!model.HasRisks)
            {
                model.Risks.Add(new ReleaseRiskItem
                {
                    EvidenceCode = "SYSTEM",
                    Title = "No deterministic risk indicators detected",
                    Description =
                        "The supplied release item information was checked for predefined risk indicators.",
                    Reason =
                        "No configured high or medium risk keywords were found in the supplied release information.",
                    Severity = "None"
                });
            }

            if (!model.HasLimitations)
            {
                model.Limitations.Add(new ReleaseRiskItem
                {
                    EvidenceCode = "SYSTEM",
                    Title = "No explicit limitations detected",
                    Description =
                        "The supplied release item information was checked for limitation and known-issue indicators.",
                    Reason =
                        "No configured limitation indicators were found in the supplied release information.",
                    Severity = "None"
                });
            }

            return View(model);
        }

        // ==========================================
        // USER IMPACT CLASSIFICATION
        // ==========================================
        public async Task<IActionResult> UserImpact(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            var items = await _context.ReleaseItems
                .Where(x => x.ReleaseId == id)
                .OrderBy(x => x.ReleaseItemId)
                .ToListAsync();

            var results = new List<UserImpactResult>();

            foreach (var item in items)
            {
                var text = string.Join(" ",
                    item.ItemType ?? "",
                    item.Title ?? "",
                    item.Description ?? "",
                    item.AffectedUsers ?? "")
                    .ToLower();

                string classification;
                string reason;

                if (text.Contains("security") ||
                    text.Contains("authentication") ||
                    text.Contains("authorization") ||
                    text.Contains("payment") ||
                    text.Contains("data loss") ||
                    text.Contains("breaking change") ||
                    text.Contains("breaking") ||
                    text.Contains("critical") ||
                    text.Contains("all users"))
                {
                    classification = "High";

                    reason =
                        "This release item may significantly affect users, security, access, payments, or critical functionality.";
                }
                else if (text.Contains("workflow") ||
                         text.Contains("performance") ||
                         text.Contains("database") ||
                         text.Contains("migration") ||
                         text.Contains("configuration") ||
                         text.Contains("api") ||
                         text.Contains("integration") ||
                         text.Contains("behaviour") ||
                         text.Contains("behavior") ||
                         text.Contains("change") ||
                         text.Contains("major"))
                {
                    classification = "Medium";

                    reason =
                        "This release item changes functionality, performance, data handling, or an important application workflow.";
                }
                else
                {
                    classification = "Low";

                    reason =
                        "This release item appears to have limited direct user impact based on the supplied release information.";
                }

                results.Add(new UserImpactResult
                {
                    ReleaseItemId = item.ReleaseItemId,
                    EvidenceCode = item.EvidenceCode ?? "",
                    Title = item.Title ?? "",
                    Classification = classification,
                    Reason = reason,
                    AffectedUsers = item.AffectedUsers ?? ""
                });
            }

            var model = new UserImpactViewModel
            {
                ReleaseId = release.ReleaseId,
                VersionNumber = release.VersionNumber,
                Items = results
            };

            return View(model);
        }

        // ==========================================
        // RELEASE VERSION HISTORY
        // ==========================================
        public async Task<IActionResult> VersionHistory(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            var history = await _context.ReleaseVersionHistories
                .Where(x => x.ReleaseId == id)
                .OrderByDescending(x => x.SnapshotCreatedAt)
                .ToListAsync();

            var model = new ReleaseVersionHistoryViewModel
            {
                ReleaseId = release.ReleaseId,
                CurrentVersion = release.VersionNumber,
                History = history
            };

            return View(model);
        }

        // ==========================================
        // CREATE RELEASE
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ReleaseId,VersionNumber,Status")]
            Release release)
        {
            if (ModelState.IsValid)
            {
                _context.Releases.Add(release);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(release);
        }

        // ==========================================
        // EDIT RELEASE
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            return View(release);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ReleaseId,VersionNumber,Status")]
            Release release)
        {
            if (id != release.ReleaseId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingRelease = await _context.Releases
                    .FirstOrDefaultAsync(x => x.ReleaseId == id);

                if (existingRelease == null)
                    return NotFound();

                // LOAD RELEASE ITEMS
                var releaseItems = await _context.ReleaseItems
                    .Where(x => x.ReleaseId == id)
                    .ToListAsync();

                // LOAD QA EVIDENCE
                var qaEvidence = await _context.Qaevidences
                    .Where(x => x.ReleaseId == id)
                    .ToListAsync();

                // PRESERVE CURRENT VERSION
                var versionHistory = new ReleaseVersionHistory
                {
                    ReleaseId = existingRelease.ReleaseId,
                    VersionNumber = existingRelease.VersionNumber,

                    CompletedFeatures =
                        existingRelease.CompletedFeatures,

                    BugFixes =
                        existingRelease.BugFixes,

                    ChangedBehaviour =
                        existingRelease.ChangedBehaviour,

                    Qasummary =
                        existingRelease.Qasummary,

                    KnownLimitations =
                        existingRelease.KnownLimitations,

                    MigrationConfigurationNotes =
                        existingRelease.MigrationConfigurationNotes,

                    AffectedUserGroups =
                        existingRelease.AffectedUserGroups,

                    Status =
                        existingRelease.Status,

                    SnapshotCreatedAt =
                        DateTime.UtcNow,

                    ReleaseItemsSnapshot =
                        System.Text.Json.JsonSerializer.Serialize(
                            releaseItems.Select(x => new
                            {
                                x.ReleaseItemId,
                                x.EvidenceCode,
                                x.ItemType,
                                x.Title,
                                x.Description,
                                x.UserImpact,
                                x.ImpactReason,
                                x.AffectedUsers
                            })),

                    QAEvidenceSnapshot =
                        System.Text.Json.JsonSerializer.Serialize(
                            qaEvidence.Select(x => new
                            {
                                x.QaevidenceId,
                                x.EvidenceCode,
                                x.TestArea,
                                x.EvidenceDescription,
                                x.Result,
                                x.TestDate
                            }))
                };

                _context.ReleaseVersionHistories
                    .Add(versionHistory);

                // UPDATE CURRENT RELEASE
                existingRelease.VersionNumber =
                    release.VersionNumber;

                existingRelease.Status =
                    release.Status;

                existingRelease.UpdatedAt =
                    DateTime.UtcNow;

                // MARK GENERATED STATEMENTS STALE
                var generatedStatements =
                    await _context.GeneratedStatements
                        .Where(x =>
                            x.ReleaseId == id &&
                            !x.IsStale)
                        .ToListAsync();

                foreach (var statement in generatedStatements)
                {
                    statement.IsStale = true;

                    statement.StaleReason =
                        "The release package was changed after this statement was generated. Review the statement again.";

                    if (string.IsNullOrWhiteSpace(
                        statement.Status))
                    {
                        statement.Status = "Draft";
                    }

                    statement.UpdatedAt =
                        DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(release);
        }

        // ==========================================
        // DELETE RELEASE
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            return View(release);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            _context.Releases.Remove(release);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // RELEASE EXISTS
        // ==========================================
        private bool ReleaseExists(int id)
        {
            return _context.Releases.Any(x => x.ReleaseId == id);
        }

        // ==========================================
        // INTERNAL TECHNICAL SUMMARY
        // ==========================================
        public async Task<IActionResult> TechnicalSummary(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            var items = await _context.ReleaseItems
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var qaEvidence = await _context.Qaevidences
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var model = new TechnicalSummaryViewModel
            {
                ReleaseId = release.ReleaseId,
                VersionNumber = release.VersionNumber
            };

            var summary = new List<string>();

            // FEATURES
            var features = items
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.ItemType) &&
                    x.ItemType.ToLower().Contains("feature"))
                .ToList();

            if (features.Any())
            {
                summary.Add("Completed Features:");

                foreach (var feature in features)
                {
                    summary.Add(
                        $"- {feature.Title}: {feature.Description}");

                    if (!string.IsNullOrWhiteSpace(
                        feature.EvidenceCode))
                    {
                        model.EvidenceReferences.Add(
                            feature.EvidenceCode);
                    }
                }
            }

            // BUG FIXES
            var bugFixes = items
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.ItemType) &&
                    (x.ItemType.ToLower().Contains("bug") ||
                     x.ItemType.ToLower().Contains("fix")))
                .ToList();

            if (bugFixes.Any())
            {
                summary.Add("");
                summary.Add("Bug Fixes:");

                foreach (var bug in bugFixes)
                {
                    summary.Add(
                        $"- {bug.Title}: {bug.Description}");

                    if (!string.IsNullOrWhiteSpace(
                        bug.EvidenceCode))
                    {
                        model.EvidenceReferences.Add(
                            bug.EvidenceCode);
                    }
                }
            }

            // CHANGED BEHAVIOUR
            var behaviourChanges = items
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.ItemType) &&
                    (x.ItemType.ToLower().Contains("behavior") ||
                     x.ItemType.ToLower().Contains("behaviour") ||
                     x.ItemType.ToLower().Contains("change")))
                .ToList();

            if (behaviourChanges.Any())
            {
                summary.Add("");
                summary.Add("Changed Behaviour:");

                foreach (var change in behaviourChanges)
                {
                    summary.Add(
                        $"- {change.Title}: {change.Description}");

                    if (!string.IsNullOrWhiteSpace(
                        change.EvidenceCode))
                    {
                        model.EvidenceReferences.Add(
                            change.EvidenceCode);
                    }
                }
            }

            // QA SUMMARY
            if (qaEvidence.Any())
            {
                summary.Add("");
                summary.Add("QA Summary:");

                foreach (var qa in qaEvidence)
                {
                    summary.Add(
                        $"- {qa.TestArea}: {qa.Result} - {qa.EvidenceDescription}");

                    if (!string.IsNullOrWhiteSpace(
                        qa.EvidenceCode))
                    {
                        model.EvidenceReferences.Add(
                            qa.EvidenceCode);
                    }
                }
            }

            // RISKS
            foreach (var item in items)
            {
                var text = string.Join(" ",
                    item.Title ?? "",
                    item.Description ?? "",
                    item.UserImpact ?? "",
                    item.ImpactReason ?? "",
                    item.AffectedUsers ?? "")
                    .ToLower();

                if (text.Contains("security") ||
                    text.Contains("authentication") ||
                    text.Contains("authorization") ||
                    text.Contains("data loss") ||
                    text.Contains("breaking change") ||
                    text.Contains("critical"))
                {
                    model.Risks.Add(
                        $"{item.Title}: High-risk change indicator detected.");
                }
                else if (text.Contains("performance") ||
                         text.Contains("migration") ||
                         text.Contains("configuration") ||
                         text.Contains("api") ||
                         text.Contains("integration") ||
                         text.Contains("workflow"))
                {
                    model.Risks.Add(
                        $"{item.Title}: Potential workflow, integration, or configuration risk.");
                }
            }

            // LIMITATIONS
            foreach (var item in items)
            {
                var text = string.Join(" ",
                    item.Title ?? "",
                    item.Description ?? "",
                    item.UserImpact ?? "",
                    item.ImpactReason ?? "",
                    item.AffectedUsers ?? "")
                    .ToLower();

                if (text.Contains("limitation") ||
                    text.Contains("known issue") ||
                    text.Contains("not supported") ||
                    text.Contains("unsupported") ||
                    text.Contains("workaround") ||
                    text.Contains("manual"))
                {
                    model.Limitations.Add(
                        $"{item.Title}: Known limitation or operational constraint.");
                }
            }

            if (!summary.Any())
            {
                summary.Add(
                    "No release information is currently available for the technical summary.");
            }

            model.Summary = string.Join(
                Environment.NewLine,
                summary);

            model.EvidenceReferences =
                model.EvidenceReferences
                    .Distinct()
                    .ToList();

            return View(model);
        }

        // ==========================================
        // RELEASE VERSION COMPARISON
        // ==========================================
        public async Task<IActionResult> Compare(
            int? releaseAId,
            int? releaseBId)
        {
            var releases = await _context.Releases
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            if (!releaseAId.HasValue ||
                !releaseBId.HasValue)
            {
                ViewBag.Releases = releases;

                return View();
            }

            var releaseA = await _context.Releases
                .Include(r => r.ReleaseItems)
                .Include(r => r.Qaevidences)
                .FirstOrDefaultAsync(
                    r => r.ReleaseId == releaseAId.Value);

            var releaseB = await _context.Releases
                .Include(r => r.ReleaseItems)
                .Include(r => r.Qaevidences)
                .FirstOrDefaultAsync(
                    r => r.ReleaseId == releaseBId.Value);

            if (releaseA == null ||
                releaseB == null)
                return NotFound();

            var viewModel = new ReleaseComparisonViewModel
            {
                ReleaseA = releaseA,
                ReleaseB = releaseB
            };

            // ADDED ITEMS
            viewModel.AddedItems =
                releaseB.ReleaseItems
                    .Where(b =>
                        !releaseA.ReleaseItems
                            .Any(a =>
                                a.EvidenceCode ==
                                b.EvidenceCode))
                    .ToList();

            // REMOVED ITEMS
            viewModel.RemovedItems =
                releaseA.ReleaseItems
                    .Where(a =>
                        !releaseB.ReleaseItems
                            .Any(b =>
                                b.EvidenceCode ==
                                a.EvidenceCode))
                    .ToList();

            // CHANGED ITEMS
            viewModel.ChangedItems =
                releaseB.ReleaseItems
                    .Where(b =>
                    {
                        var a = releaseA.ReleaseItems
                            .FirstOrDefault(x =>
                                x.EvidenceCode ==
                                b.EvidenceCode);

                        return a != null &&
                               (
                                   a.Title != b.Title ||
                                   a.Description != b.Description ||
                                   a.UserImpact != b.UserImpact ||
                                   a.ImpactReason != b.ImpactReason ||
                                   a.AffectedUsers != b.AffectedUsers ||
                                   a.ItemType != b.ItemType
                               );
                    })
                    .ToList();

            // ADDED QA
            viewModel.AddedQaEvidence =
                releaseB.Qaevidences
                    .Where(b =>
                        !releaseA.Qaevidences
                            .Any(a =>
                                a.EvidenceCode ==
                                b.EvidenceCode))
                    .ToList();

            // REMOVED QA
            viewModel.RemovedQaEvidence =
                releaseA.Qaevidences
                    .Where(a =>
                        !releaseB.Qaevidences
                            .Any(b =>
                                b.EvidenceCode ==
                                a.EvidenceCode))
                    .ToList();

            // PACKAGE COMPARISON
            if (releaseA.CompletedFeatures !=
                releaseB.CompletedFeatures)
            {
                viewModel.ChangedPackageFields.Add(
                    "Completed Features");
            }

            if (releaseA.BugFixes !=
                releaseB.BugFixes)
            {
                viewModel.ChangedPackageFields.Add(
                    "Bug Fixes");
            }

            if (releaseA.ChangedBehaviour !=
                releaseB.ChangedBehaviour)
            {
                viewModel.ChangedPackageFields.Add(
                    "Changed Behaviour");
            }

            if (releaseA.Qasummary !=
                releaseB.Qasummary)
            {
                viewModel.ChangedPackageFields.Add(
                    "QA Summary");
            }

            if (releaseA.KnownLimitations !=
                releaseB.KnownLimitations)
            {
                viewModel.ChangedPackageFields.Add(
                    "Known Limitations");
            }

            if (releaseA.MigrationConfigurationNotes !=
                releaseB.MigrationConfigurationNotes)
            {
                viewModel.ChangedPackageFields.Add(
                    "Migration / Configuration Notes");
            }

            if (releaseA.AffectedUserGroups !=
                releaseB.AffectedUserGroups)
            {
                viewModel.ChangedPackageFields.Add(
                    "Affected User Groups");
            }

            ViewBag.Releases = releases;

            return View(viewModel);
        }

        // ==========================================
        // AI MISSING INFORMATION ANALYSIS
        // ==========================================
        public async Task<IActionResult> AiMissingInformation(
            int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            var items = await _context.ReleaseItems
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var qaEvidence = await _context.Qaevidences
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var releaseInformation = $"""
                Version: {release.VersionNumber}

                Completed Features:
                {release.CompletedFeatures}

                Bug Fixes:
                {release.BugFixes}

                Changed Behaviour:
                {release.ChangedBehaviour}

                QA Summary:
                {release.Qasummary}

                Known Limitations:
                {release.KnownLimitations}

                Migration / Configuration Notes:
                {release.MigrationConfigurationNotes}

                Affected User Groups:
                {release.AffectedUserGroups}

                Release Items:
                {string.Join(Environment.NewLine, items.Select(x =>
                    $"{x.EvidenceCode} | {x.ItemType} | {x.Title} | " +
                    $"{x.Description} | Impact: {x.UserImpact} | " +
                    $"Affected Users: {x.AffectedUsers}"))}

                QA Evidence:
                {string.Join(Environment.NewLine, qaEvidence.Select(x =>
                    $"{x.EvidenceCode} | {x.TestArea} | " +
                    $"Result: {x.Result} | {x.EvidenceDescription} | " +
                    $"Test Date: {x.TestDate}"))}
                """;

            var analysis =
                await _openAIService
                    .AnalyzeMissingInformationAsync(
                        release.VersionNumber,
                        releaseInformation);

            ViewBag.VersionNumber =
                release.VersionNumber;

            ViewBag.Analysis =
                analysis;

            return View();
        }

        // ==========================================
        // AI UNSUPPORTED CLAIM DETECTION
        // ==========================================
        public async Task<IActionResult> AiUnsupportedClaims(
            int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            var statement = await _context.GeneratedStatements
                .Where(x => x.ReleaseId == id)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (statement == null)
            {
                ViewBag.VersionNumber =
                    release.VersionNumber;

                ViewBag.Analysis =
                    "No generated statement is available for this release.";

                return View();
            }

            var qaEvidence = await _context.Qaevidences
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var evidenceText = string.Join(
                Environment.NewLine,
                qaEvidence.Select(x =>
                    $"{x.EvidenceCode} | " +
                    $"{x.TestArea} | " +
                    $"Result: {x.Result} | " +
                    $"{x.EvidenceDescription} | " +
                    $"Test Date: {x.TestDate}")
            );

            var analysis =
                await _openAIService
                    .AnalyzeUnsupportedClaimsAsync(
                        statement.StatementText,
                        evidenceText);

            ViewBag.VersionNumber =
                release.VersionNumber;

            ViewBag.StatementText =
                statement.StatementText;

            ViewBag.Analysis =
                analysis;

            return View();
        }

        // ==========================================
        // FINAL RELEASE BRIEF
        // ==========================================
        public async Task<IActionResult> FinalBrief(int? id)
        {
            if (id == null)
                return NotFound();

            var release = await _context.Releases
                .FirstOrDefaultAsync(x => x.ReleaseId == id);

            if (release == null)
                return NotFound();

            var items = await _context.ReleaseItems
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var qaEvidence = await _context.Qaevidences
                .Where(x => x.ReleaseId == id)
                .ToListAsync();

            var statements = await _context.GeneratedStatements
                .Where(x => x.ReleaseId == id)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            var reviewedStatements = statements
                .Where(x =>
                    x.Status == "Approved" ||
                    x.Status == "Edited")
                .ToList();

            var model = new FinalReleaseBriefViewModel
            {
                ReleaseId = release.ReleaseId,
                VersionNumber = release.VersionNumber,
                Status = release.Status,
                ReviewedStatements = reviewedStatements,

                HasUnapprovedStatements =
                    statements.Any(x =>
                        x.Status != "Approved" &&
                        x.Status != "Edited"),

                HasStaleStatements =
                    statements.Any(x =>
                        x.IsStale)
            };

            // ==========================================
            // AI TECHNICAL + STAKEHOLDER SUMMARIES
            // ==========================================

            var releaseInformation = $"""
                Release Version: {release.VersionNumber}

                Completed Features:
                {release.CompletedFeatures}

                Bug Fixes:
                {release.BugFixes}

                Changed Behaviour:
                {release.ChangedBehaviour}

                QA Summary:
                {release.Qasummary}

                Known Limitations:
                {release.KnownLimitations}

                Migration / Configuration Notes:
                {release.MigrationConfigurationNotes}

                Affected User Groups:
                {release.AffectedUserGroups}

                Release Items:
                {string.Join(Environment.NewLine, items.Select(x =>
                    $"{x.EvidenceCode} | {x.ItemType} | {x.Title} | " +
                    $"{x.Description} | Impact: {x.UserImpact} | " +
                    $"Reason: {x.ImpactReason} | " +
                    $"Affected Users: {x.AffectedUsers}"))}

                QA Evidence:
                {string.Join(Environment.NewLine, qaEvidence.Select(x =>
                    $"{x.EvidenceCode} | {x.TestArea} | " +
                    $"Result: {x.Result} | " +
                    $"{x.EvidenceDescription} | " +
                    $"Test Date: {x.TestDate}"))}
                """;

            // ==========================================
            // AI TECHNICAL SUMMARY
            // ==========================================

            var technicalSummary =
                await _openAIService
                    .GenerateTechnicalSummaryAsync(
                        release.VersionNumber,
                        releaseInformation);

            // ==========================================
            // AI STAKEHOLDER SUMMARY
            // ==========================================

            var stakeholderSummary =
                await _openAIService
                    .GenerateStakeholderSummaryAsync(
                        release.VersionNumber,
                        releaseInformation);

            model.TechnicalSummary =
                technicalSummary;

            model.StakeholderSummary =
                stakeholderSummary;

            // ==========================================
            // EVIDENCE REFERENCES
            // ==========================================

            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(
                    item.EvidenceCode))
                {
                    model.EvidenceReferences.Add(
                        item.EvidenceCode);
                }
            }

            foreach (var qa in qaEvidence)
            {
                if (!string.IsNullOrWhiteSpace(
                    qa.EvidenceCode))
                {
                    model.EvidenceReferences.Add(
                        qa.EvidenceCode);
                }
            }

            // ==========================================
            // RISKS AND LIMITATIONS
            // ==========================================

            foreach (var item in items)
            {
                var text = string.Join(" ",
                    item.Title ?? "",
                    item.Description ?? "",
                    item.UserImpact ?? "",
                    item.ImpactReason ?? "",
                    item.AffectedUsers ?? "")
                    .ToLower();

                // HIGH RISK
                if (text.Contains("security") ||
                    text.Contains("authentication") ||
                    text.Contains("authorization") ||
                    text.Contains("data loss") ||
                    text.Contains("breaking change") ||
                    text.Contains("critical"))
                {
                    model.Risks.Add(
                        $"{item.Title}: High-risk change indicator detected.");
                }

                // MEDIUM RISK
                else if (text.Contains("performance") ||
                         text.Contains("migration") ||
                         text.Contains("configuration") ||
                         text.Contains("api") ||
                         text.Contains("integration") ||
                         text.Contains("workflow"))
                {
                    model.Risks.Add(
                        $"{item.Title}: Potential workflow, integration, or configuration risk.");
                }

                // LIMITATIONS
                if (text.Contains("limitation") ||
                    text.Contains("known issue") ||
                    text.Contains("not supported") ||
                    text.Contains("unsupported") ||
                    text.Contains("workaround") ||
                    text.Contains("manual"))
                {
                    model.Limitations.Add(
                        $"{item.Title}: Known limitation or operational constraint.");
                }
            }

            // ==========================================
            // REMOVE DUPLICATE EVIDENCE REFERENCES
            // ==========================================

            model.EvidenceReferences =
                model.EvidenceReferences
                    .Distinct()
                    .ToList();

            return View(model);
        }
    }
}