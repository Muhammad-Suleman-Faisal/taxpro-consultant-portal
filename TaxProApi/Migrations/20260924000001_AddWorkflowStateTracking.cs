using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxProApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowStateTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new fields to Payments table
            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedByAdminId",
                table: "Payments",
                type: "integer",
                nullable: true);

            // Add new fields to Consultations table
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Consultations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Consultations",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove fields from Payments table
            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RejectedByAdminId",
                table: "Payments");

            // Remove fields from Consultations table
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Consultations");
        }
    }
}
