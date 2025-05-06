#nullable disable

namespace TgStorage.Migrations.TgEfBlazor;

/// <inheritdoc />
public partial class UpdatedLicenses_RemovedRowVersion : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "LICENSES");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "LICENSES",
            type: "BLOB",
            rowVersion: true,
            nullable: true);
    }
}
