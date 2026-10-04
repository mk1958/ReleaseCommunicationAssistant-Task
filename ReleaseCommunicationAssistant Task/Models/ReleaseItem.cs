using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReleaseCommunicationAssistant_Task.Models;

[Index("ReleaseId", Name = "IX_ReleaseItems_ReleaseId")]
[Index("ReleaseId", "EvidenceCode", Name = "UQ_ReleaseItems_EvidenceCode", IsUnique = true)]
public partial class ReleaseItem
{
    [Key]
    public int ReleaseItemId { get; set; }

    public int ReleaseId { get; set; }

    [StringLength(50)]
    public string EvidenceCode { get; set; } = null!;

    [StringLength(30)]
    public string ItemType { get; set; } = null!;

    [StringLength(250)]
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    [StringLength(30)]
    public string? UserImpact { get; set; }

    public string? ImpactReason { get; set; }

    [StringLength(500)]
    public string? AffectedUsers { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("ReleaseId")]
    [InverseProperty("ReleaseItems")]
    public virtual Release Release { get; set; } = null!;
}
