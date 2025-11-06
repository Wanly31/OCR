using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCR.Migrations.OCRDb
{
    /// <inheritdoc />
    public partial class addtablerecognizedtext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedDocuments_Documents_DocumentId",
                table: "RecognizedDocuments");

            migrationBuilder.DropIndex(
                name: "IX_RecognizedDocuments_DocumentId",
                table: "RecognizedDocuments");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "RecognizedDocuments",
                newName: "RecognizedTextId");

            migrationBuilder.CreateTable(
                name: "RecognizedText",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognizedText", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecognizedText_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecognizedDocuments_RecognizedTextId",
                table: "RecognizedDocuments",
                column: "RecognizedTextId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecognizedText_DocumentId",
                table: "RecognizedText",
                column: "DocumentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedDocuments_RecognizedText_RecognizedTextId",
                table: "RecognizedDocuments",
                column: "RecognizedTextId",
                principalTable: "RecognizedText",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecognizedDocuments_RecognizedText_RecognizedTextId",
                table: "RecognizedDocuments");

            migrationBuilder.DropTable(
                name: "RecognizedText");

            migrationBuilder.DropIndex(
                name: "IX_RecognizedDocuments_RecognizedTextId",
                table: "RecognizedDocuments");

            migrationBuilder.RenameColumn(
                name: "RecognizedTextId",
                table: "RecognizedDocuments",
                newName: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RecognizedDocuments_DocumentId",
                table: "RecognizedDocuments",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecognizedDocuments_Documents_DocumentId",
                table: "RecognizedDocuments",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
