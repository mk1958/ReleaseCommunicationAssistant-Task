namespace ReleaseCommunicationAssistant_Task.Models
{
    public class FinalReleaseBriefViewModel
    {
        public int ReleaseId { get; set; }

        public string VersionNumber { get; set; } = "";

        public string Status { get; set; } = "";

        public string TechnicalSummary { get; set; } = "";

        public string StakeholderSummary { get; set; } = "";

        public List<string> EvidenceReferences { get; set; } = new();

        public List<string> Risks { get; set; } = new();

        public List<string> Limitations { get; set; } = new();

        public List<GeneratedStatements> ReviewedStatements { get; set; } = new();

        public bool HasUnapprovedStatements { get; set; }

        public bool HasStaleStatements { get; set; }
    }
}