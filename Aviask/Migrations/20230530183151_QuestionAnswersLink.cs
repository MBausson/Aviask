using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aviask.Migrations
{
    /// <inheritdoc />
    public partial class QuestionAnswersLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionAnswersId",
                table: "Question",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionAnswersId",
                table: "Question",
                column: "QuestionAnswersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionAnswers_QuestionAnswersId",
                table: "Question",
                column: "QuestionAnswersId",
                principalTable: "QuestionAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionAnswers_QuestionAnswersId",
                table: "Question");

            migrationBuilder.DropIndex(
                name: "IX_Question_QuestionAnswersId",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "QuestionAnswersId",
                table: "Question");
        }
    }
}
