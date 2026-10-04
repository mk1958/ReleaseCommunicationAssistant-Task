using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReleaseCommunicationAssistant_Task.Migrations
{
    /// <inheritdoc />
    public partial class AddReleaseVersionSnapshotData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QAEvidenceSnapshot",
                table: "ReleaseVersionHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseItemsSnapshot",
                table: "ReleaseVersionHistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QAEvidenceSnapshot",
                table: "ReleaseVersionHistories");

            migrationBuilder.DropColumn(
                name: "ReleaseItemsSnapshot",
                table: "ReleaseVersionHistories");
        }
    }
}
