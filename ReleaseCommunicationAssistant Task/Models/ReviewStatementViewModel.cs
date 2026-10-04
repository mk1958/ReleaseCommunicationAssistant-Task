namespace ReleaseCommunicationAssistant_Task.Models
{
    public class ReviewStatementViewModel
    {
        public int GeneratedStatementId { get; set; }

        public int ReleaseId { get; set; }

        public string VersionNumber { get; set; } = "";

        public string Audience { get; set; } = "";

        public string StatementType { get; set; } = "";

        public string StatementText { get; set; } = "";

        public string ImpactLevel { get; set; } = "";

        public string EvidenceReferences { get; set; } = "";

        public bool IsSupported { get; set; }

        public bool IsStale { get; set; }

        public string? StaleReason { get; set; }

        public string Status { get; set; } = "";

        public string? EditedText { get; set; }

        public string? ReviewComment { get; set; }

        public string? ReviewerName { get; set; }
    }
}