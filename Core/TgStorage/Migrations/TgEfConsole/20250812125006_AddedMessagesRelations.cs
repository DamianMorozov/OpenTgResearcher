#nullable disable

namespace TgStorage.Migrations.TgEfConsole;

/// <inheritdoc />
public partial class AddedMessagesRelations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_STORIES_ID",
            table: "STORIES");

        migrationBuilder.DropColumn(
            name: "PARENT_ID",
            table: "MESSAGES");

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "VERSIONS",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "USERS",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AlterColumn<long>(
            name: "FROM_ID",
            table: "STORIES",
            type: "LONG(20)",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "LONG(20)",
            oldNullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "STORIES",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "SOURCES",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "PROXIES",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "MESSAGES",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "LICENSES",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "FILTERS",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "DOCUMENTS",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "APPS",
            type: "BLOB",
            rowVersion: true,
            nullable: true);

        migrationBuilder.AddUniqueConstraint(
            name: "AK_VERSIONS_VERSION",
            table: "VERSIONS",
            column: "VERSION");

        migrationBuilder.AddUniqueConstraint(
            name: "AK_USERS_ID",
            table: "USERS",
            column: "ID");

        migrationBuilder.AddUniqueConstraint(
            name: "AK_STORIES_ID_FROM_ID",
            table: "STORIES",
            columns: new[] { "ID", "FROM_ID" });

        migrationBuilder.AddUniqueConstraint(
            name: "AK_MESSAGES_SOURCE_ID_ID",
            table: "MESSAGES",
            columns: new[] { "SOURCE_ID", "ID" });

        migrationBuilder.AddUniqueConstraint(
            name: "AK_LICENSES_LICENSE_KEY",
            table: "LICENSES",
            column: "LICENSE_KEY");

        migrationBuilder.AddUniqueConstraint(
            name: "AK_APPS_API_HASH",
            table: "APPS",
            column: "API_HASH");

        migrationBuilder.CreateTable(
            name: "MESSAGES_RELATIONS",
            columns: table => new
            {
                UID = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                PARENT_SOURCE_ID = table.Column<long>(type: "LONG(20)", nullable: false),
                PARENT_MESSAGE_ID = table.Column<int>(type: "INT", nullable: false),
                CHILD_SOURCE_ID = table.Column<long>(type: "LONG(20)", nullable: false),
                CHILD_MESSAGE_ID = table.Column<int>(type: "INT", nullable: false),
                RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MESSAGES_RELATIONS", x => x.UID);
                table.ForeignKey(
                    name: "FK_MESSAGES_RELATIONS_MESSAGES_CHILD_SOURCE_ID_CHILD_MESSAGE_ID",
                    columns: x => new { x.CHILD_SOURCE_ID, x.CHILD_MESSAGE_ID },
                    principalTable: "MESSAGES",
                    principalColumns: new[] { "SOURCE_ID", "ID" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_MESSAGES_RELATIONS_MESSAGES_PARENT_SOURCE_ID_PARENT_MESSAGE_ID",
                    columns: x => new { x.PARENT_SOURCE_ID, x.PARENT_MESSAGE_ID },
                    principalTable: "MESSAGES",
                    principalColumns: new[] { "SOURCE_ID", "ID" },
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_MESSAGES_RELATIONS_SOURCES_CHILD_SOURCE_ID",
                    column: x => x.CHILD_SOURCE_ID,
                    principalTable: "SOURCES",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_MESSAGES_RELATIONS_SOURCES_PARENT_SOURCE_ID",
                    column: x => x.PARENT_SOURCE_ID,
                    principalTable: "SOURCES",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_ID",
            table: "STORIES",
            column: "ID");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_RELATIONS_CHILD_MESSAGE_ID",
            table: "MESSAGES_RELATIONS",
            column: "CHILD_MESSAGE_ID");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_RELATIONS_CHILD_SOURCE_ID",
            table: "MESSAGES_RELATIONS",
            column: "CHILD_SOURCE_ID");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_RELATIONS_CHILD_SOURCE_ID_CHILD_MESSAGE_ID",
            table: "MESSAGES_RELATIONS",
            columns: new[] { "CHILD_SOURCE_ID", "CHILD_MESSAGE_ID" });

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_RELATIONS_PARENT_MESSAGE_ID",
            table: "MESSAGES_RELATIONS",
            column: "PARENT_MESSAGE_ID");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_RELATIONS_PARENT_SOURCE_ID",
            table: "MESSAGES_RELATIONS",
            column: "PARENT_SOURCE_ID");

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_RELATIONS_UID",
            table: "MESSAGES_RELATIONS",
            column: "UID",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_RELATIONS_UNIQUE_LINK",
            table: "MESSAGES_RELATIONS",
            columns: new[] { "PARENT_SOURCE_ID", "PARENT_MESSAGE_ID", "CHILD_SOURCE_ID", "CHILD_MESSAGE_ID" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MESSAGES_RELATIONS");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_VERSIONS_VERSION",
            table: "VERSIONS");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_USERS_ID",
            table: "USERS");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_STORIES_ID_FROM_ID",
            table: "STORIES");

        migrationBuilder.DropIndex(
            name: "IX_STORIES_ID",
            table: "STORIES");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_MESSAGES_SOURCE_ID_ID",
            table: "MESSAGES");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_LICENSES_LICENSE_KEY",
            table: "LICENSES");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_APPS_API_HASH",
            table: "APPS");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "VERSIONS");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "USERS");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "STORIES");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "SOURCES");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "PROXIES");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "MESSAGES");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "LICENSES");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "FILTERS");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "DOCUMENTS");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "APPS");

        migrationBuilder.AlterColumn<long>(
            name: "FROM_ID",
            table: "STORIES",
            type: "LONG(20)",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "LONG(20)");

        migrationBuilder.AddColumn<int>(
            name: "PARENT_ID",
            table: "MESSAGES",
            type: "INT",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_STORIES_ID",
            table: "STORIES",
            column: "ID",
            unique: true);
    }
}
