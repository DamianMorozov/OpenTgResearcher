#nullable disable

namespace TgStorage.Migrations.TgEfBlazor;

/// <inheritdoc />
public partial class AddedLicenses : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LICENSES",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                LICENSE_KEY = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                USER_ID = table.Column<long>(type: "LONG(20)", nullable: false),
                LICENSE_TYPE = table.Column<int>(type: "INT(1)", nullable: false),
                VALID_TO = table.Column<DateTime>(type: "DATETIME", nullable: false),
                IS_CONFIRMED = table.Column<bool>(type: "BIT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LICENSES", x => x.UID);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LICENSES_IS_CONFIRMED",
            table: "LICENSES",
            column: "IS_CONFIRMED");

        migrationBuilder.CreateIndex(
            name: "IX_LICENSES_LICENSE_KEY",
            table: "LICENSES",
            column: "LICENSE_KEY",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LICENSES_LICENSE_TYPE",
            table: "LICENSES",
            column: "LICENSE_TYPE");

        migrationBuilder.CreateIndex(
            name: "IX_LICENSES_UID",
            table: "LICENSES",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LICENSES_USER_ID",
            table: "LICENSES",
            column: "USER_ID");

        migrationBuilder.CreateIndex(
            name: "IX_LICENSES_VALID_TO",
            table: "LICENSES",
            column: "VALID_TO");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LICENSES");
    }
}
