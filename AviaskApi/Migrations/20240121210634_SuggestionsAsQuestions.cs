using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AviaskApi.Migrations
{
    /// <inheritdoc />
    public partial class SuggestionsAsQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionSuggestions");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Questions");

            migrationBuilder.CreateTable(
                name: "QuestionSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublisherId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuestionAnswersId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IllustrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSuggestions_AspNetUsers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionSuggestions_Attachments_IllustrationId",
                        column: x => x.IllustrationId,
                        principalTable: "Attachments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionSuggestions_QuestionAnswers_QuestionAnswersId",
                        column: x => x.QuestionAnswersId,
                        principalTable: "QuestionAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSuggestions_IllustrationId",
                table: "QuestionSuggestions",
                column: "IllustrationId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSuggestions_PublisherId",
                table: "QuestionSuggestions",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSuggestions_QuestionAnswersId",
                table: "QuestionSuggestions",
                column: "QuestionAnswersId");
        }
    }
}
