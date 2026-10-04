namespace ReleaseCommunicationAssistant_Task.Models
{
    public class QaClaimCheckViewModel
    {
        public int ReleaseItemId { get; set; }

        public int ReleaseId { get; set; }

        public string EvidenceCode { get; set; } = string.Empty;

        public string Claim { get; set; } = string.Empty;

        public bool IsSupported { get; set; }

        public string Result { get; set; } = string.Empty;

        public string EvidenceDescription { get; set; } = string.Empty;

        public string TestArea { get; set; } = string.Empty;

        public string SupportReason { get; set; } = string.Empty;
    }
}