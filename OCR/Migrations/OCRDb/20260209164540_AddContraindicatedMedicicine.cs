using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCR.Migrations.OCRDb
{
    /// <inheritdoc />
    public partial class AddContraindicatedMedicicine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "RecognizedDocuments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContraindicatedMedicine",
                table: "RecognizedDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContraindicatedReason",
                table: "RecognizedDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "RecognizedDocuments");

            migrationBuilder.DropColumn(
                name: "ContraindicatedMedicine",
                table: "RecognizedDocuments");

            migrationBuilder.DropColumn(
                name: "ContraindicatedReason",
                table: "RecognizedDocuments");
        }
    }
}
