namespace ReleaseCommunicationAssistant_Task.Models
{
    public class QASupportViewModel
    {
        public int GeneratedStatementId { get; set; }

        public int ReleaseId { get; set; }

        public string VersionNumber { get; set; } = "";

        public string StatementCode { get; set; } = "";

        public string StatementText { get; set; } = "";

        public string EvidenceReferences { get; set; } = "";

        public bool IsSupported { get; set; }

        public string SupportStatus { get; set; } = "";

        public string AnalysisReason { get; set; } = "";

        public List<QASupportEvidence> SupportingEvidence { get; set; } = new();

        public List<string> UnsupportedClaims { get; set; } = new();
    }

    public class QASupportEvidence
    {
        public string EvidenceCode { get; set; } = "";

        public string TestArea { get; set; } = "";

        public string Result { get; set; } = "";

        public string EvidenceDescription { get; set; } = "";
    }
}