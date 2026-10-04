using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReleaseCommunicationAssistant_Task.Models;

[Table("QAEvidences")]
[Index("EvidenceCode", Name = "IX_QAEvidences_EvidenceCode")]
[Index("ReleaseId", Name = "IX_QAEvidences_ReleaseId")]
[Index("ReleaseId", "EvidenceCode", Name = "UQ_QAEvidences_EvidenceCode", IsUnique = true)]
public partial class Qaevidence
{
    [Key]
    [Column("QAEvidenceId")]
    public int QaevidenceId { get; set; }

    public int ReleaseId { get; set; }

    [StringLength(50)]
    public string EvidenceCode { get; set; } = null!;

    [StringLength(250)]
    public string TestArea { get; set; } = null!;

    public string EvidenceDescription { get; set; } = null!;

    [StringLength(30)]
    public string Result { get; set; } = null!;

    public DateTime? TestDate { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("ReleaseId")]
    [InverseProperty("Qaevidences")]
    public virtual Release Release { get; set; } = null!;
}
