using Microsoft.EntityFrameworkCore;

namespace ReleaseCommunicationAssistant_Task.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GeneratedStatements> GeneratedStatements { get; set; }

    public virtual DbSet<Qaevidence> Qaevidences { get; set; }

    public virtual DbSet<Release> Releases { get; set; }

    public virtual DbSet<ReleaseItem> ReleaseItems { get; set; }

    public virtual DbSet<ReviewDecision> ReviewDecisions { get; set; }

    public virtual DbSet<ReleaseVersionHistory> ReleaseVersionHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeneratedStatements>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.IsSupported)
                .HasDefaultValue(true);

            entity.Property(e => e.Status)
                .HasDefaultValue("Draft");

            entity.HasOne(d => d.Release)
                .WithMany(p => p.GeneratedStatements)
                .HasConstraintName("FK_GeneratedStatements_Releases");
        });

        modelBuilder.Entity<Qaevidence>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Release)
                .WithMany(p => p.Qaevidences)
                .HasConstraintName("FK_QAEvidences_Releases");
        });

        modelBuilder.Entity<Release>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Status)
                .HasDefaultValue("Draft");
        });

        modelBuilder.Entity<ReleaseItem>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Release)
                .WithMany(p => p.ReleaseItems)
                .HasConstraintName("FK_ReleaseItems_Releases");
        });

        modelBuilder.Entity<ReviewDecision>(entity =>
        {
            entity.Property(e => e.ReviewedAt)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.GeneratedStatement)
                .WithMany(p => p.ReviewDecisions)
                .HasConstraintName("FK_ReviewDecisions_GeneratedStatements");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}