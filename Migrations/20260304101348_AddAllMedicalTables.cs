using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INF_SP.Migrations
{
    /// <inheritdoc />
    public partial class AddAllMedicalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Users_DoctorId",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Users_PatientId",
                table: "MedicalRecords");

            migrationBuilder.AddColumn<string>(
                name: "ReasonForVisit",
                table: "MedicalRecords",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "HealthProblems",
                columns: table => new
                {
                    ProblemID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProblemDescription = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DiagnosedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthProblems", x => x.ProblemID);
                    table.ForeignKey(
                        name: "FK_HealthProblems_Users_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    PatientID = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Instructions = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.PrescriptionID);
                    table.ForeignKey(
                        name: "FK_Prescriptions_MedicalRecords_RecordId",
                        column: x => x.RecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Users_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VitalsLogs",
                columns: table => new
                {
                    VitalID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    PatientID = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight_kg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Height_cm = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    BMI = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    RecordedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VitalsLogs", x => x.VitalID);
                    table.ForeignKey(
                        name: "FK_VitalsLogs_MedicalRecords_RecordId",
                        column: x => x.RecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VitalsLogs_Users_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentGoals",
                columns: table => new
                {
                    GoalID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProblemID = table.Column<int>(type: "INTEGER", nullable: false),
                    GoalDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ProgressPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    SetDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentGoals", x => x.GoalID);
                    table.ForeignKey(
                        name: "FK_TreatmentGoals_HealthProblems_ProblemID",
                        column: x => x.ProblemID,
                        principalTable: "HealthProblems",
                        principalColumn: "ProblemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthProblems_PatientID",
                table: "HealthProblems",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientID",
                table: "Prescriptions",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_RecordId",
                table: "Prescriptions",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentGoals_ProblemID",
                table: "TreatmentGoals",
                column: "ProblemID");

            migrationBuilder.CreateIndex(
                name: "IX_VitalsLogs_PatientID",
                table: "VitalsLogs",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_VitalsLogs_RecordId",
                table: "VitalsLogs",
                column: "RecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Users_DoctorId",
                table: "MedicalRecords",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Users_PatientId",
                table: "MedicalRecords",
                column: "PatientId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Users_DoctorId",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Users_PatientId",
                table: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "TreatmentGoals");

            migrationBuilder.DropTable(
                name: "VitalsLogs");

            migrationBuilder.DropTable(
                name: "HealthProblems");

            migrationBuilder.DropColumn(
                name: "ReasonForVisit",
                table: "MedicalRecords");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Users_DoctorId",
                table: "MedicalRecords",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Users_PatientId",
                table: "MedicalRecords",
                column: "PatientId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
