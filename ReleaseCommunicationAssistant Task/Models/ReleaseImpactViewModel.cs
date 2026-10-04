namespace ReleaseCommunicationAssistant_Task.Models
{
    public class ReleaseImpactViewModel
    {
        public int ReleaseItemId { get; set; }

        public string EvidenceCode { get; set; } = string.Empty;

        public string ItemType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string UserImpact { get; set; } = string.Empty;

        public string ImpactReason { get; set; } = string.Empty;

        public string AffectedUsers { get; set; } = string.Empty;

        public string ImpactLevel { get; set; } = string.Empty;
    }
}