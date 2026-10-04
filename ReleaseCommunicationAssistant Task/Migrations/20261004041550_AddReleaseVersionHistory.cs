using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReleaseCommunicationAssistant_Task.Migrations
{
    /// <inheritdoc />
    public partial class AddReleaseVersionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleaseVersionHistories",
                columns: table => new
                {
                    ReleaseVersionHistoryId = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    ReleaseId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    VersionNumber = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),

                    CompletedFeatures = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    BugFixes = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    ChangedBehaviour = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Qasummary = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    KnownLimitations = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    MigrationConfigurationNotes = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    AffectedUserGroups = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Status = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: true),

                    SnapshotCreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ReleaseVersionHistories",
                        x => x.ReleaseVersionHistoryId);

                    table.ForeignKey(
                        name: "FK_ReleaseVersionHistories_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "ReleaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseVersionHistories_ReleaseId",
                table: "ReleaseVersionHistories",
                column: "ReleaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseVersionHistories");
        }
    }
}