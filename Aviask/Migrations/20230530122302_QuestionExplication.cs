using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aviask.Migrations
{
    /// <inheritdoc />
    public partial class QuestionExplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Explications",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Explications",
                table: "Question");
        }
    }
}
