using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCR.Migrations.OCRDb
{
    /// <inheritdoc />
    public partial class AddExamination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Examination",
                table: "RecognizedDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Examination",
                table: "RecognizedDocuments");
        }
    }
}
