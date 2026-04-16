using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INF_SP.Migrations
{
    /// <inheritdoc />
    public partial class RemovePrescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prescription",
                table: "MedicalRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prescription",
                table: "MedicalRecords",
                type: "TEXT",
                nullable: true);
        }
    }
}
