namespace ReleaseCommunicationAssistant_Task.Models
{
    public class ReleaseComparisonViewModel
    {
        public Release ReleaseA { get; set; } = null!;

        public Release ReleaseB { get; set; } = null!;

        public List<ReleaseItem> AddedItems { get; set; } = new();

        public List<ReleaseItem> RemovedItems { get; set; } = new();

        public List<ReleaseItem> ChangedItems { get; set; } = new();

        public List<Qaevidence> AddedQaEvidence { get; set; } = new();

        public List<Qaevidence> RemovedQaEvidence { get; set; } = new();

        public List<string> ChangedPackageFields { get; set; } = new();
    }
}