#nullable disable

namespace TgStorage.Migrations.TgEfBlazor;

/// <inheritdoc />
public partial class UpdatedSources_AddedIsCreatingSubdirectories : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IS_CREATING_SUBDIRS",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_CREATING_SUBDIRS",
            table: "SOURCES",
            column: "IS_CREATING_SUBDIRS");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_CREATING_SUBDIRS",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_CREATING_SUBDIRS",
            table: "SOURCES");
    }
}
