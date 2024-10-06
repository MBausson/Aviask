using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AviaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IllustrationId",
                table: "QuestionSuggestions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IllustrationId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSuggestions_IllustrationId",
                table: "QuestionSuggestions",
                column: "IllustrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_IllustrationId",
                table: "Questions",
                column: "IllustrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Attachments_IllustrationId",
                table: "Questions",
                column: "IllustrationId",
                principalTable: "Attachments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSuggestions_Attachments_IllustrationId",
                table: "QuestionSuggestions",
                column: "IllustrationId",
                principalTable: "Attachments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Attachments_IllustrationId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSuggestions_Attachments_IllustrationId",
                table: "QuestionSuggestions");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_QuestionSuggestions_IllustrationId",
                table: "QuestionSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_IllustrationId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "IllustrationId",
                table: "QuestionSuggestions");

            migrationBuilder.DropColumn(
                name: "IllustrationId",
                table: "Questions");
        }
    }
}
