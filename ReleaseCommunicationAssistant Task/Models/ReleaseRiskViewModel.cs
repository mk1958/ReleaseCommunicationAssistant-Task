namespace ReleaseCommunicationAssistant_Task.Models
{
    public class ReleaseRiskViewModel
    {
        public int ReleaseId { get; set; }

        public string VersionNumber { get; set; } = string.Empty;

        public List<ReleaseRiskItem> Risks { get; set; } = new();

        public List<ReleaseRiskItem> Limitations { get; set; } = new();

        public bool HasRisks => Risks.Any();

        public bool HasLimitations => Limitations.Any();
    }

    public class ReleaseRiskItem
    {
        public string EvidenceCode { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;
    }
}