#nullable disable

namespace TgStorage.Migrations.TgEfConsole;

/// <inheritdoc />
public partial class AddedUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CONTACTS");

        migrationBuilder.CreateTable(
            name: "USERS",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                DT_CHANGED = table.Column<DateTime>(type: "DATETIME", nullable: false),
                ID = table.Column<long>(type: "LONG(20)", nullable: false),
                ACCESS_HASH = table.Column<long>(type: "LONG(20)", nullable: false),
                IS_ACTIVE = table.Column<bool>(type: "BIT", nullable: false),
                IS_BOT = table.Column<bool>(type: "BIT", nullable: false),
                FIRST_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                LAST_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                USER_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                USER_NAMES = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                PHONE_NUMBER = table.Column<string>(type: "NVARCHAR(20)", maxLength: 20, nullable: true),
                STATUS = table.Column<string>(type: "NVARCHAR(20)", maxLength: 20, nullable: true),
                RESTRICTION_REASON = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                LANG_CODE = table.Column<string>(type: "NVARCHAR(16)", maxLength: 16, nullable: true),
                STORIES_MAX_ID = table.Column<int>(type: "INT(20)", nullable: false),
                BOT_INFO_VERSION = table.Column<string>(type: "NVARCHAR(20)", maxLength: 20, nullable: true),
                BOT_INLINE_PLACEHOLDER = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                BOT_ACTIVE_USERS = table.Column<int>(type: "INT(20)", nullable: false),
                IS_CONTACT = table.Column<bool>(type: "BIT", nullable: false),
                IS_DELETED = table.Column<bool>(type: "BIT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_USERS", x => x.UID);
            });

        migrationBuilder.CreateIndex(
            name: "IX_USERS_ACCESS_HASH",
            table: "USERS",
            column: "ACCESS_HASH");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_DT_CHANGED",
            table: "USERS",
            column: "DT_CHANGED");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_FIRST_NAME",
            table: "USERS",
            column: "FIRST_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_ID",
            table: "USERS",
            column: "ID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_USERS_IS_ACTIVE",
            table: "USERS",
            column: "IS_ACTIVE");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_IS_BOT",
            table: "USERS",
            column: "IS_BOT");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_LANG_CODE",
            table: "USERS",
            column: "LANG_CODE");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_LAST_NAME",
            table: "USERS",
            column: "LAST_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_PHONE_NUMBER",
            table: "USERS",
            column: "PHONE_NUMBER");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_STATUS",
            table: "USERS",
            column: "STATUS");

        migrationBuilder.CreateIndex(
            name: "IX_USERS_UID",
            table: "USERS",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_USERS_USER_NAME",
            table: "USERS",
            column: "USER_NAME");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "USERS");

        migrationBuilder.CreateTable(
            name: "CONTACTS",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                ACCESS_HASH = table.Column<long>(type: "LONG(20)", nullable: false),
                BOT_ACTIVE_USERS = table.Column<int>(type: "INT(20)", nullable: false),
                BOT_INFO_VERSION = table.Column<string>(type: "NVARCHAR(20)", maxLength: 20, nullable: true),
                BOT_INLINE_PLACEHOLDER = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                DT_CHANGED = table.Column<DateTime>(type: "DATETIME", nullable: false),
                FIRST_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                ID = table.Column<long>(type: "LONG(20)", nullable: false),
                IS_ACTIVE = table.Column<bool>(type: "BIT", nullable: false),
                IS_BOT = table.Column<bool>(type: "BIT", nullable: false),
                LANG_CODE = table.Column<string>(type: "NVARCHAR(16)", maxLength: 16, nullable: true),
                LAST_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                PHONE_NUMBER = table.Column<string>(type: "NVARCHAR(20)", maxLength: 20, nullable: true),
                RESTRICTION_REASON = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                STATUS = table.Column<string>(type: "NVARCHAR(20)", maxLength: 20, nullable: true),
                STORIES_MAX_ID = table.Column<int>(type: "INT(20)", nullable: false),
                USER_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true),
                USER_NAMES = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CONTACTS", x => x.UID);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_ACCESS_HASH",
            table: "CONTACTS",
            column: "ACCESS_HASH");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_DT_CHANGED",
            table: "CONTACTS",
            column: "DT_CHANGED");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_FIRST_NAME",
            table: "CONTACTS",
            column: "FIRST_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_ID",
            table: "CONTACTS",
            column: "ID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_IS_ACTIVE",
            table: "CONTACTS",
            column: "IS_ACTIVE");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_IS_BOT",
            table: "CONTACTS",
            column: "IS_BOT");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_LANG_CODE",
            table: "CONTACTS",
            column: "LANG_CODE");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_LAST_NAME",
            table: "CONTACTS",
            column: "LAST_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_PHONE_NUMBER",
            table: "CONTACTS",
            column: "PHONE_NUMBER");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_STATUS",
            table: "CONTACTS",
            column: "STATUS");

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_UID",
            table: "CONTACTS",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CONTACTS_USER_NAME",
            table: "CONTACTS",
            column: "USER_NAME");
    }
}
