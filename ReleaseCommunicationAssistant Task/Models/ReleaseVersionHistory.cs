using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReleaseCommunicationAssistant_Task.Models
{
    public class ReleaseVersionHistory
    {
        [Key]
        public int ReleaseVersionHistoryId { get; set; }

        public int ReleaseId { get; set; }

        [StringLength(50)]
        public string VersionNumber { get; set; } = "";

        public string? CompletedFeatures { get; set; }

        public string? BugFixes { get; set; }

        public string? ChangedBehaviour { get; set; }

        public string? Qasummary { get; set; }

        public string? KnownLimitations { get; set; }

        public string? MigrationConfigurationNotes { get; set; }

        public string? AffectedUserGroups { get; set; }

        [StringLength(30)]
        public string? Status { get; set; }

        public DateTime SnapshotCreatedAt { get; set; }

        // Preserved release items
        public string? ReleaseItemsSnapshot { get; set; }

        // Preserved QA evidence
        public string? QAEvidenceSnapshot { get; set; }

        [ForeignKey("ReleaseId")]
        public virtual Release Release { get; set; } = null!;
    }
}