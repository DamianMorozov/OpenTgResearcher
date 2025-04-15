#nullable disable

namespace TgStorage.Migrations.TgEfDesktop;

/// <inheritdoc />
public partial class UpdatedSources_AddedIsUserAccess : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IS_USER_ACCESS",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_USER_ACCESS",
            table: "SOURCES",
            column: "IS_USER_ACCESS");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_USER_ACCESS",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_USER_ACCESS",
            table: "SOURCES");
    }
}
