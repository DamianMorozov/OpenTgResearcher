#nullable disable

namespace TgStorage.Migrations.TgEfTest;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FILTERS",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                IS_ENABLED = table.Column<bool>(type: "BIT", nullable: false),
                FILTER_TYPE = table.Column<int>(type: "INT", nullable: false),
                NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                MASK = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                SIZE = table.Column<long>(type: "LONG(20)", nullable: false),
                SIZE_TYPE = table.Column<int>(type: "INT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FILTERS", x => x.UID);
            });

        migrationBuilder.CreateTable(
            name: "PROXIES",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                TYPE = table.Column<int>(type: "INT", nullable: false),
                HOST_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                PORT = table.Column<ushort>(type: "INT(5)", nullable: false),
                USER_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                PASSWORD = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false),
                SECRET = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PROXIES", x => x.UID);
            });

        migrationBuilder.CreateTable(
            name: "SOURCES",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                ID = table.Column<long>(type: "LONG(20)", nullable: false),
                USER_NAME = table.Column<string>(type: "NVARCHAR(128)", maxLength: 256, nullable: true),
                TITLE = table.Column<string>(type: "NVARCHAR(256)", maxLength: 1024, nullable: true),
                ABOUT = table.Column<string>(type: "NVARCHAR(1024)", nullable: true),
                COUNT = table.Column<int>(type: "INT", nullable: false),
                DIRECTORY = table.Column<string>(type: "NVARCHAR(256)", maxLength: 1024, nullable: true),
                FIRST_ID = table.Column<int>(type: "INT", nullable: false),
                IS_AUTO_UPDATE = table.Column<bool>(type: "BIT", nullable: false),
                DT_CHANGED = table.Column<DateTime>(type: "DATETIME", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SOURCES", x => x.UID);
                table.UniqueConstraint("AK_SOURCES_ID", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "VERSIONS",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                VERSION = table.Column<short>(type: "SMALLINT", maxLength: 4, nullable: false),
                DESCRIPTION = table.Column<string>(type: "NVARCHAR(128)", maxLength: 128, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VERSIONS", x => x.UID);
            });

        migrationBuilder.CreateTable(
            name: "APPS",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                API_HASH = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                API_ID = table.Column<int>(type: "INT", nullable: false),
                PHONE_NUMBER = table.Column<string>(type: "CHAR(20)", maxLength: 20, nullable: false),
                PROXY_UID = table.Column<Guid>(type: "CHAR(36)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_APPS", x => x.UID);
                table.ForeignKey(
                    name: "FK_APPS_PROXIES_PROXY_UID",
                    column: x => x.PROXY_UID,
                    principalTable: "PROXIES",
                    principalColumn: "UID");
            });

        migrationBuilder.CreateTable(
            name: "DOCUMENTS",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                SOURCE_ID = table.Column<long>(type: "LONG(20)", nullable: true),
                ID = table.Column<long>(type: "LONG(20)", nullable: false),
                MESSAGE_ID = table.Column<long>(type: "LONG(20)", nullable: false),
                FILE_NAME = table.Column<string>(type: "NVARCHAR(256)", maxLength: 256, nullable: false),
                FILE_SIZE = table.Column<long>(type: "LONG(20)", nullable: false),
                ACCESS_HASH = table.Column<long>(type: "LONG(20)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DOCUMENTS", x => x.UID);
                table.ForeignKey(
                    name: "FK_DOCUMENTS_SOURCES_SOURCE_ID",
                    column: x => x.SOURCE_ID,
                    principalTable: "SOURCES",
                    principalColumn: "ID");
            });

        migrationBuilder.CreateTable(
            name: "MESSAGES",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                SOURCE_ID = table.Column<long>(type: "LONG(20)", nullable: true),
                ID = table.Column<long>(type: "LONG(20)", nullable: false),
                DT_CREATED = table.Column<DateTime>(type: "DATETIME", nullable: false),
                TYPE = table.Column<int>(type: "INT", nullable: false),
                SIZE = table.Column<long>(type: "LONG(20)", nullable: false),
                MESSAGE = table.Column<string>(type: "NVARCHAR(100)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MESSAGES", x => x.UID);
                table.ForeignKey(
                    name: "FK_MESSAGES_SOURCES_SOURCE_ID",
                    column: x => x.SOURCE_ID,
                    principalTable: "SOURCES",
                    principalColumn: "ID");
            });

        migrationBuilder.CreateIndex(
            name: "IX_APPS_API_HASH",
            table: "APPS",
            column: "API_HASH",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_APPS_API_ID",
            table: "APPS",
            column: "API_ID");

        migrationBuilder.CreateIndex(
            name: "IX_APPS_PHONE_NUMBER",
            table: "APPS",
            column: "PHONE_NUMBER");

        migrationBuilder.CreateIndex(
            name: "IX_APPS_PROXY_UID",
            table: "APPS",
            column: "PROXY_UID");

        migrationBuilder.CreateIndex(
            name: "IX_APPS_UID",
            table: "APPS",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DOCUMENTS_ACCESS_HASH",
            table: "DOCUMENTS",
            column: "ACCESS_HASH");

        migrationBuilder.CreateIndex(
            name: "IX_DOCUMENTS_FILE_NAME",
            table: "DOCUMENTS",
            column: "FILE_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_DOCUMENTS_FILE_SIZE",
            table: "DOCUMENTS",
            column: "FILE_SIZE");

        migrationBuilder.CreateIndex(
            name: "IX_DOCUMENTS_ID",
            table: "DOCUMENTS",
            column: "ID");

        migrationBuilder.CreateIndex(
            name: "IX_DOCUMENTS_MESSAGE_ID",
            table: "DOCUMENTS",
            column: "MESSAGE_ID");

        migrationBuilder.CreateIndex(
            name: "IX_DOCUMENTS_SOURCE_ID",
            table: "DOCUMENTS",
            column: "SOURCE_ID");

        migrationBuilder.CreateIndex(
            name: "IX_DOCUMENTS_UID",
            table: "DOCUMENTS",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_FILTERS_FILTER_TYPE",
            table: "FILTERS",
            column: "FILTER_TYPE");

        migrationBuilder.CreateIndex(
            name: "IX_FILTERS_IS_ENABLED",
            table: "FILTERS",
            column: "IS_ENABLED");

        migrationBuilder.CreateIndex(
            name: "IX_FILTERS_MASK",
            table: "FILTERS",
            column: "MASK");

        migrationBuilder.CreateIndex(
            name: "IX_FILTERS_NAME",
            table: "FILTERS",
            column: "NAME");

        migrationBuilder.CreateIndex(
            name: "IX_FILTERS_SIZE",
            table: "FILTERS",
            column: "SIZE");

        migrationBuilder.CreateIndex(
            name: "IX_FILTERS_SIZE_TYPE",
            table: "FILTERS",
            column: "SIZE_TYPE");

        migrationBuilder.CreateIndex(
            name: "IX_FILTERS_UID",
            table: "FILTERS",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_DT_CREATED",
            table: "MESSAGES",
            column: "DT_CREATED");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_ID",
            table: "MESSAGES",
            column: "ID");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_MESSAGE",
            table: "MESSAGES",
            column: "MESSAGE");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_SIZE",
            table: "MESSAGES",
            column: "SIZE");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_SOURCE_ID",
            table: "MESSAGES",
            column: "SOURCE_ID");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_TYPE",
            table: "MESSAGES",
            column: "TYPE");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_UID",
            table: "MESSAGES",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PROXIES_HOST_NAME",
            table: "PROXIES",
            column: "HOST_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_PROXIES_PASSWORD",
            table: "PROXIES",
            column: "PASSWORD");

        migrationBuilder.CreateIndex(
            name: "IX_PROXIES_PORT",
            table: "PROXIES",
            column: "PORT");

        migrationBuilder.CreateIndex(
            name: "IX_PROXIES_SECRET",
            table: "PROXIES",
            column: "SECRET");

        migrationBuilder.CreateIndex(
            name: "IX_PROXIES_TYPE",
            table: "PROXIES",
            column: "TYPE");

        migrationBuilder.CreateIndex(
            name: "IX_PROXIES_UID",
            table: "PROXIES",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PROXIES_USER_NAME",
            table: "PROXIES",
            column: "USER_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_COUNT",
            table: "SOURCES",
            column: "COUNT");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_DIRECTORY",
            table: "SOURCES",
            column: "DIRECTORY");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_DT_CHANGED",
            table: "SOURCES",
            column: "DT_CHANGED");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_FIRST_ID",
            table: "SOURCES",
            column: "FIRST_ID");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_ID",
            table: "SOURCES",
            column: "ID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_IS_AUTO_UPDATE",
            table: "SOURCES",
            column: "IS_AUTO_UPDATE");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_TITLE",
            table: "SOURCES",
            column: "TITLE");

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_UID",
            table: "SOURCES",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SOURCES_USER_NAME",
            table: "SOURCES",
            column: "USER_NAME");

        migrationBuilder.CreateIndex(
            name: "IX_VERSIONS_DESCRIPTION",
            table: "VERSIONS",
            column: "DESCRIPTION");

        migrationBuilder.CreateIndex(
            name: "IX_VERSIONS_UID",
            table: "VERSIONS",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_VERSIONS_VERSION",
            table: "VERSIONS",
            column: "VERSION",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "APPS");

        migrationBuilder.DropTable(
            name: "DOCUMENTS");

        migrationBuilder.DropTable(
            name: "FILTERS");

        migrationBuilder.DropTable(
            name: "MESSAGES");

        migrationBuilder.DropTable(
            name: "VERSIONS");

        migrationBuilder.DropTable(
            name: "PROXIES");

        migrationBuilder.DropTable(
            name: "SOURCES");
    }
}
