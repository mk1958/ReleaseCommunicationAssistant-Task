namespace ReleaseCommunicationAssistant_Task.Models
{
    public class TechnicalSummaryViewModel
    {
        public int ReleaseId { get; set; }

        public string VersionNumber { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<string> EvidenceReferences { get; set; } = new();

        public List<string> Risks { get; set; } = new();

        public List<string> Limitations { get; set; } = new();
    }
}