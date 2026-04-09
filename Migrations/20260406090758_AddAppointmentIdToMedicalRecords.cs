using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INF_SP.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentIdToMedicalRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "MedicalRecords",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "MedicalRecords");
        }
    }
}
