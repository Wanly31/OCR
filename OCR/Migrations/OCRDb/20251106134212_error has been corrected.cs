using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCR.Migrations.OCRDb
{
    /// <inheritdoc />
    public partial class errorhasbeencorrected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedDocuments_RecognizedText_RecognizedTextId",
                table: "RecognizedDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedText_Documents_DocumentId",
                table: "RecognizedText");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecognizedText",
                table: "RecognizedText");

            migrationBuilder.RenameTable(
                name: "RecognizedText",
                newName: "RecognizedTexts");

            migrationBuilder.RenameIndex(
                name: "IX_RecognizedText_DocumentId",
                table: "RecognizedTexts",
                newName: "IX_RecognizedTexts_DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecognizedTexts",
                table: "RecognizedTexts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedDocuments_RecognizedTexts_RecognizedTextId",
                table: "RecognizedDocuments",
                column: "RecognizedTextId",
                principalTable: "RecognizedTexts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedTexts_Documents_DocumentId",
                table: "RecognizedTexts",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedDocuments_RecognizedTexts_RecognizedTextId",
                table: "RecognizedDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedTexts_Documents_DocumentId",
                table: "RecognizedTexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecognizedTexts",
                table: "RecognizedTexts");

            migrationBuilder.RenameTable(
                name: "RecognizedTexts",
                newName: "RecognizedText");

            migrationBuilder.RenameIndex(
                name: "IX_RecognizedTexts_DocumentId",
                table: "RecognizedText",
                newName: "IX_RecognizedText_DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecognizedText",
                table: "RecognizedText",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedDocuments_RecognizedText_RecognizedTextId",
                table: "RecognizedDocuments",
                column: "RecognizedTextId",
                principalTable: "RecognizedText",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedText_Documents_DocumentId",
                table: "RecognizedText",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
