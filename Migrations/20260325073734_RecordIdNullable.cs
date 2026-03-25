using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INF_SP.Migrations
{
    /// <inheritdoc />
    public partial class RecordIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VitalsLogs_MedicalRecords_RecordId",
                table: "VitalsLogs");

            migrationBuilder.AlterColumn<int>(
                name: "RecordId",
                table: "VitalsLogs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_VitalsLogs_MedicalRecords_RecordId",
                table: "VitalsLogs",
                column: "RecordId",
                principalTable: "MedicalRecords",
                principalColumn: "RecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VitalsLogs_MedicalRecords_RecordId",
                table: "VitalsLogs");

            migrationBuilder.AlterColumn<int>(
                name: "RecordId",
                table: "VitalsLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VitalsLogs_MedicalRecords_RecordId",
                table: "VitalsLogs",
                column: "RecordId",
                principalTable: "MedicalRecords",
                principalColumn: "RecordId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
