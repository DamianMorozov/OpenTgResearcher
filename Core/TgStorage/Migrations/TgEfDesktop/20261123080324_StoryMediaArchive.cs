using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TgStorage.Migrations.TgEfDesktop
{
    /// <inheritdoc />
    public partial class StoryMediaArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IS_CHANNEL",
                table: "STORIES",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IS_DELETED",
                table: "STORIES",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MEDIA_PATH",
                table: "STORIES",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PEER_ACCESS_HASH",
                table: "STORIES",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IS_CHANNEL",
                table: "STORIES");

            migrationBuilder.DropColumn(
                name: "IS_DELETED",
                table: "STORIES");

            migrationBuilder.DropColumn(
                name: "MEDIA_PATH",
                table: "STORIES");

            migrationBuilder.DropColumn(
                name: "PEER_ACCESS_HASH",
                table: "STORIES");
        }
    }
}
