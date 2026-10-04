namespace ReleaseCommunicationAssistant_Task.Models
{
    public class UserImpactViewModel
    {
        public int ReleaseId { get; set; }

        public string VersionNumber { get; set; } = "";

        public List<UserImpactResult> Items { get; set; } = new();
    }

    public class UserImpactResult
    {
        public int ReleaseItemId { get; set; }

        public string EvidenceCode { get; set; } = "";

        public string Title { get; set; } = "";

        public string Classification { get; set; } = "";

        public string Reason { get; set; } = "";

        public string AffectedUsers { get; set; } = "";
    }
}