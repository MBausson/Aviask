using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aviask.Migrations
{
    /// <inheritdoc />
    public partial class AnswerRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnswerRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Answered = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerRecords_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_QuestionId",
                table: "AnswerRecords",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerRecords");
        }
    }
}
