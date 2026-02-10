using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCR.Migrations.OCRDb
{
    /// <inheritdoc />
    public partial class AddPatientAndRefactorRecognizeText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "RecognizedDocuments");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "RecognizedDocuments");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "RecognizedDocuments");

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "RecognizedDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecognizedDocuments_PatientId",
                table: "RecognizedDocuments",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedDocuments_Patients_PatientId",
                table: "RecognizedDocuments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedDocuments_Patients_PatientId",
                table: "RecognizedDocuments");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_RecognizedDocuments_PatientId",
                table: "RecognizedDocuments");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "RecognizedDocuments");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "RecognizedDocuments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "RecognizedDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "RecognizedDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
