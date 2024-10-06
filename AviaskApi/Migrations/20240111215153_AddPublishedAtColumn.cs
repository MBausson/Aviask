using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AviaskApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Questions",
                type: "timestamp with time zone",
                nullable: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Questions");
        }
    }
}
