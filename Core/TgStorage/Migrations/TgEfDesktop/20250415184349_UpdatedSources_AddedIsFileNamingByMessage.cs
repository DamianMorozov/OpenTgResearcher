#nullable disable

namespace TgStorage.Migrations.TgEfDesktop;

/// <inheritdoc />
public partial class UpdatedSources_AddedIsFileNamingByMessage : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<bool>(
			name: "IS_FILE_NAMING_BY_MESSAGE",
			table: "SOURCES",
			type: "BIT",
			nullable: false,
			defaultValue: false);

		migrationBuilder.CreateIndex(
			name: "IX_SOURCES_IS_FILE_NAMING_BY_MESSAGE",
			table: "SOURCES",
			column: "IS_FILE_NAMING_BY_MESSAGE");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "IX_SOURCES_IS_FILE_NAMING_BY_MESSAGE",
			table: "SOURCES");

		migrationBuilder.DropColumn(
			name: "IS_FILE_NAMING_BY_MESSAGE",
			table: "SOURCES");
	}
}
