#nullable disable

namespace TgStorage.Migrations.TgEfDesktop
{
    /// <inheritdoc />
    public partial class UpdatedSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ACCESS_HASH",
                table: "SOURCES",
                type: "LONG(20)",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IS_ACTIVE",
                table: "SOURCES",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SOURCES_ACCESS_HASH",
                table: "SOURCES",
                column: "ACCESS_HASH");

            migrationBuilder.CreateIndex(
                name: "IX_SOURCES_IS_ACTIVE",
                table: "SOURCES",
                column: "IS_ACTIVE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SOURCES_ACCESS_HASH",
                table: "SOURCES");

            migrationBuilder.DropIndex(
                name: "IX_SOURCES_IS_ACTIVE",
                table: "SOURCES");

            migrationBuilder.DropColumn(
                name: "ACCESS_HASH",
                table: "SOURCES");

            migrationBuilder.DropColumn(
                name: "IS_ACTIVE",
                table: "SOURCES");
        }
    }
}
