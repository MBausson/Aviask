using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aviask.Migrations
{
    /// <inheritdoc />
    public partial class QuestionPublisher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublisherId",
                table: "Question",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Question_PublisherId",
                table: "Question",
                column: "PublisherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_AspNetUsers_PublisherId",
                table: "Question",
                column: "PublisherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_AspNetUsers_PublisherId",
                table: "Question");

            migrationBuilder.DropIndex(
                name: "IX_Question_PublisherId",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "Question");
        }
    }
}
