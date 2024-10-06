using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AviaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMockExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MockExamId",
                table: "AnswerRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MockExams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    MaxDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    MaxQuestions = table.Column<int>(type: "integer", nullable: false),
                    CorrectnessRatio = table.Column<float>(type: "real", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockExams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockExams_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MockExams_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_MockExamId",
                table: "AnswerRecords",
                column: "MockExamId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExams_QuestionId",
                table: "MockExams",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_MockExams_UserId",
                table: "MockExams",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerRecords_MockExams_MockExamId",
                table: "AnswerRecords",
                column: "MockExamId",
                principalTable: "MockExams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerRecords_MockExams_MockExamId",
                table: "AnswerRecords");

            migrationBuilder.DropTable(
                name: "MockExams");

            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_MockExamId",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "MockExamId",
                table: "AnswerRecords");
        }
    }
}
