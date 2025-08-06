#nullable disable

namespace TgStorage.Migrations.TgEfDesktop;

/// <inheritdoc />
public partial class UpdatedSources_AddedIsSubscribe : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IS_SUBSCRIBE",
            table: "SOURCES",
            type: "BIT",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_SUBSCRIBE",
            table: "SOURCES",
            column: "IS_SUBSCRIBE");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SOURCES_IS_SUBSCRIBE",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "IS_SUBSCRIBE",
            table: "SOURCES");
    }
}
