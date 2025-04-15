#nullable disable

namespace TgStorage.Migrations.TgEfDesktop;

/// <inheritdoc />
public partial class AddedStories : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "STORIES",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                DT_CHANGED = table.Column<DateTime>(type: "DATETIME", nullable: false),
                ID = table.Column<long>(type: "LONG(20)", nullable: false),
                FROM_ID = table.Column<long>(type: "LONG(20)", nullable: true),
                FROM_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                DATE = table.Column<DateTime>(type: "DATETIME", nullable: true),
                EXPIRE_DATE = table.Column<DateTime>(type: "DATETIME", nullable: true),
                CAPTION = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                TYPE = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                OFFSET = table.Column<int>(type: "INT(20)", nullable: false),
                LENGTH = table.Column<int>(type: "INT(20)", nullable: false),
                MESSAGE = table.Column<string>(type: "NVARCHAR(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_STORIES", x => x.UID);
            });

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_CAPTION",
            table: "STORIES",
            column: "CAPTION");

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_DATE",
            table: "STORIES",
            column: "DATE");

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_DT_CHANGED",
            table: "STORIES",
            column: "DT_CHANGED");

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_EXPIRE_DATE",
            table: "STORIES",
            column: "EXPIRE_DATE");

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_FROM_ID",
            table: "STORIES",
            column: "FROM_ID");

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_FROM_NAME",
            table: "STORIES",
            column: "FROM_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_ID",
            table: "STORIES",
            column: "ID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_TYPE",
            table: "STORIES",
            column: "TYPE");

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_UID",
            table: "STORIES",
            column: "UID",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "STORIES");
    }
}
