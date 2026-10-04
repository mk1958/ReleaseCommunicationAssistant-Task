namespace ReleaseCommunicationAssistant_Task.Models
{
    public class ReleaseReadinessViewModel
    {
        public int ReleaseId { get; set; }

        public string VersionNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool HasCompletedFeatures { get; set; }

        public bool HasBugFixes { get; set; }

        public bool HasChangedBehaviour { get; set; }

        public bool HasQaSummary { get; set; }

        public bool HasKnownLimitations { get; set; }

        public bool HasMigrationNotes { get; set; }

        public bool HasAffectedUserGroups { get; set; }

        public bool IsComplete { get; set; }

        public List<string> MissingSections { get; set; } = new();
    }
}