using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReleaseCommunicationAssistant_Task.Models;

[Index("GeneratedStatementId", Name = "IX_ReviewDecisions_GeneratedStatementId")]
public partial class ReviewDecision
{
    [Key]
    public int ReviewDecisionId { get; set; }

    public int GeneratedStatementId { get; set; }

    [StringLength(30)]
    public string Decision { get; set; } = null!;

    public string? EditedText { get; set; }

    [StringLength(150)]
    public string? ReviewerName { get; set; }

    public string? ReviewComment { get; set; }

    public DateTime ReviewedAt { get; set; }

    [ForeignKey("GeneratedStatementId")]
    [InverseProperty("ReviewDecisions")]
    public virtual GeneratedStatements GeneratedStatement { get; set; } = null!;
}
