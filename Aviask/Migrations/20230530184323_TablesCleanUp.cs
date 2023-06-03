using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aviask.Migrations
{
    /// <inheritdoc />
    public partial class TablesCleanUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer1",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "Answer2",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "Answer3",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "Answer4",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "Explications",
                table: "Question");

            migrationBuilder.AddColumn<string>(
                name: "Explications",
                table: "QuestionAnswers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Explications",
                table: "QuestionAnswers");

            migrationBuilder.AddColumn<string>(
                name: "Answer1",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Answer2",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Answer3",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Answer4",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Explications",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
