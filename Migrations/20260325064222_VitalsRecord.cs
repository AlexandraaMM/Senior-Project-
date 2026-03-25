using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INF_SP.Migrations
{
    /// <inheritdoc />
    public partial class VitalsRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BMI",
                table: "VitalsLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BMI",
                table: "VitalsLogs",
                type: "decimal(5,2)",
                nullable: true);
        }
    }
}
