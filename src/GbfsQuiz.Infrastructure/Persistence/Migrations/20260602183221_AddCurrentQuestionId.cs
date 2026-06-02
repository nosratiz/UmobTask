using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GbfsQuiz.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentQuestionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "current_question_id",
                table: "game_sessions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_question_id",
                table: "game_sessions");
        }
    }
}
