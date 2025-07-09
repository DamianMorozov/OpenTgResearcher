#nullable disable

namespace TgStorage.Migrations.TgEfBlazor;

/// <inheritdoc />
public partial class UpdatedSources_AddedIsRestrictSavingContent : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IS_RESTRICT_SAVING_CONTENT",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_RESTRICT_SAVING_CONTENT",
            table: "SOURCES",
            column: "IS_RESTRICT_SAVING_CONTENT");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_RESTRICT_SAVING_CONTENT",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_RESTRICT_SAVING_CONTENT",
            table: "SOURCES");
    }
}
