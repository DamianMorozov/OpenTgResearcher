#nullable disable

namespace TgStorage.Migrations.TgEfDesktop;

/// <inheritdoc />
public partial class UpdateSources_AddedFlags : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "COUNT_THREADS",
            table: "SOURCES",
            type: "INT",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IS_FILE_WITH_ID",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IS_PARSING_COMMENTS",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IS_REWRITE_FILES",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IS_REWRITE_MSG",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IS_SAVE_FILES",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IS_SAVE_MSG",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IS_THUMB",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_COUNT_THREADS",
            table: "SOURCES",
            column: "COUNT_THREADS");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_FILE_WITH_ID",
            table: "SOURCES",
            column: "IS_FILE_WITH_ID");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_PARSING_COMMENTS",
            table: "SOURCES",
            column: "IS_PARSING_COMMENTS");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_REWRITE_FILES",
            table: "SOURCES",
            column: "IS_REWRITE_FILES");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_REWRITE_MSG",
            table: "SOURCES",
            column: "IS_REWRITE_MSG");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_SAVE_FILES",
            table: "SOURCES",
            column: "IS_SAVE_FILES");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_SAVE_MSG",
            table: "SOURCES",
            column: "IS_SAVE_MSG");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_THUMB",
            table: "SOURCES",
            column: "IS_THUMB");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SOURCES_COUNT_THREADS",
            table: "SOURCES");

        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_FILE_WITH_ID",
            table: "SOURCES");

        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_PARSING_COMMENTS",
            table: "SOURCES");

        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_REWRITE_FILES",
            table: "SOURCES");

        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_REWRITE_MSG",
            table: "SOURCES");

        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_SAVE_FILES",
            table: "SOURCES");

        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_SAVE_MSG",
            table: "SOURCES");

        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_THUMB",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "COUNT_THREADS",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_FILE_WITH_ID",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_PARSING_COMMENTS",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_REWRITE_FILES",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_REWRITE_MSG",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_SAVE_FILES",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_SAVE_MSG",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_THUMB",
            table: "SOURCES");
    }
}
