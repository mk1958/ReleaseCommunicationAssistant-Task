namespace ReleaseCommunicationAssistant_Task.Models
{
    public class ReleaseVersionHistoryViewModel
    {
        public int ReleaseId { get; set; }

        public string CurrentVersion { get; set; } = "";

        public List<ReleaseVersionHistory> History { get; set; } = new();
    }
}