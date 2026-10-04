using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReleaseCommunicationAssistant_Task.Models;

[Index("IsStale", Name = "IX_GeneratedStatements_IsStale")]
[Index("ReleaseId", Name = "IX_GeneratedStatements_ReleaseId")]
[Index("ReleaseId", "StatementCode", Name = "UQ_GeneratedStatements_Code", IsUnique = true)]
public partial class GeneratedStatements
{
    [Key]
    public int GeneratedStatementId { get; set; }

    public int ReleaseId { get; set; }

    [StringLength(50)]
    public string StatementCode { get; set; } = null!;

    [StringLength(30)]
    public string Audience { get; set; } = null!;

    [StringLength(50)]
    public string StatementType { get; set; } = null!;

    public string StatementText { get; set; } = null!;

    [StringLength(30)]
    public string? ImpactLevel { get; set; }

    public string? EvidenceReferences { get; set; }

    public bool IsSupported { get; set; }

    public bool IsStale { get; set; }

    public string? StaleReason { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("AIModel")]
    [StringLength(100)]
    public string? Aimodel { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("ReleaseId")]
    [InverseProperty("GeneratedStatements")]
    public virtual Release? Release { get; set; }

    [InverseProperty("GeneratedStatement")]
    public virtual ICollection<ReviewDecision> ReviewDecisions { get; set; } = new List<ReviewDecision>();
}
