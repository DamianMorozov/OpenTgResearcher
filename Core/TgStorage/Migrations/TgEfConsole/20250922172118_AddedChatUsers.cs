#nullable disable

namespace TgStorage.Migrations.TgEfConsole;

/// <inheritdoc />
public partial class AddedChatUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CHAT_USERS",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                DT_CHANGED = table.Column<DateTime>(type: "DATETIME", nullable: false),
                CHAT_ID = table.Column<long>(type: "LONG(20)", nullable: false),
                USER_ID = table.Column<long>(type: "LONG(20)", nullable: false),
                ROLE = table.Column<int>(type: "INT", nullable: false),
                JOINED_AT = table.Column<DateTime>(type: "DATETIME", nullable: false),
                IS_MUTED = table.Column<bool>(type: "BIT", nullable: false),
                MUTED_UNTIL = table.Column<DateTime>(type: "DATETIME", nullable: true),
                IS_DELETED = table.Column<bool>(type: "BIT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CHAT_USERS", x => x.UID);
                table.ForeignKey(
                    name: "FK_CHAT_USERS_SOURCES_CHAT_ID",
                    column: x => x.CHAT_ID,
                    principalTable: "SOURCES",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CHAT_USERS_USERS_USER_ID",
                    column: x => x.USER_ID,
                    principalTable: "USERS",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_USERS_IS_DELETED",
            table: "USERS",
            column: "IS_DELETED");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_IS_DELETED",
            table: "MESSAGES",
            column: "IS_DELETED");

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_CHAT_ID",
            table: "CHAT_USERS",
            column: "CHAT_ID");

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_CHAT_ID_USER_ID",
            table: "CHAT_USERS",
            columns: new[] { "CHAT_ID", "USER_ID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_DT_CHANGED",
            table: "CHAT_USERS",
            column: "DT_CHANGED");

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_IS_DELETED",
            table: "CHAT_USERS",
            column: "IS_DELETED");

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_IS_MUTED",
            table: "CHAT_USERS",
            column: "IS_MUTED");

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_JOINED_AT",
            table: "CHAT_USERS",
            column: "JOINED_AT");

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_ROLE",
            table: "CHAT_USERS",
            column: "ROLE");

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_UID",
            table: "CHAT_USERS",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_USERS_USER_ID",
            table: "CHAT_USERS",
            column: "USER_ID");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CHAT_USERS");

        migrationBuilder.DropIndex(
            name: "IX_USERS_IS_DELETED",
            table: "USERS");

        migrationBuilder.DropIndex(
            name: "IX_MESSAGES_IS_DELETED",
            table: "MESSAGES");
    }
}
