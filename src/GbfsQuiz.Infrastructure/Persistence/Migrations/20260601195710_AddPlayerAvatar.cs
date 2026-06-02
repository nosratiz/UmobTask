using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GbfsQuiz.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_content_type",
                table: "players",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "avatar_data",
                table: "players",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_content_type",
                table: "players");

            migrationBuilder.DropColumn(
                name: "avatar_data",
                table: "players");
        }
    }
}
