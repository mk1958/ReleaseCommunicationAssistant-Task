using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReleaseCommunicationAssistant_Task.Models;

[Index("VersionNumber", Name = "UQ_Releases_VersionNumber", IsUnique = true)]
public partial class Release
{
    [Key]
    public int ReleaseId { get; set; }

    [StringLength(50)]
    public string VersionNumber { get; set; } = null!;

    public string? CompletedFeatures { get; set; }

    public string? BugFixes { get; set; }

    public string? ChangedBehaviour { get; set; }

    [Column("QASummary")]
    public string? Qasummary { get; set; }

    public string? KnownLimitations { get; set; }

    public string? MigrationConfigurationNotes { get; set; }

    public string? AffectedUserGroups { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Release")]
    public virtual ICollection<GeneratedStatements> GeneratedStatements { get; set; } = new List<GeneratedStatements>();

    [InverseProperty("Release")]
    public virtual ICollection<Qaevidence> Qaevidences { get; set; } = new List<Qaevidence>();

    [InverseProperty("Release")]
    public virtual ICollection<ReleaseItem> ReleaseItems { get; set; } = new List<ReleaseItem>();
}
