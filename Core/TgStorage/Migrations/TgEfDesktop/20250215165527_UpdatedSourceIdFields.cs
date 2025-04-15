#nullable disable

namespace TgStorage.Migrations.TgEfDesktop;

/// <inheritdoc />
public partial class UpdatedSourceIdFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_DOCUMENTS_SOURCES_SOURCE_ID",
            table: "DOCUMENTS");

        migrationBuilder.DropForeignKey(
            name: "FK_MESSAGES_SOURCES_SOURCE_ID",
            table: "MESSAGES");

        migrationBuilder.AlterColumn<long>(
            name: "SOURCE_ID",
            table: "MESSAGES",
            type: "LONG(20)",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "LONG(20)",
            oldNullable: true);

        migrationBuilder.AlterColumn<long>(
            name: "SOURCE_ID",
            table: "DOCUMENTS",
            type: "LONG(20)",
            nullable: false,
            defaultValue: 0L,
            oldClrType: typeof(long),
            oldType: "LONG(20)",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_DOCUMENTS_SOURCES_SOURCE_ID",
            table: "DOCUMENTS",
            column: "SOURCE_ID",
            principalTable: "SOURCES",
            principalColumn: "ID",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_MESSAGES_SOURCES_SOURCE_ID",
            table: "MESSAGES",
            column: "SOURCE_ID",
            principalTable: "SOURCES",
            principalColumn: "ID",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_DOCUMENTS_SOURCES_SOURCE_ID",
            table: "DOCUMENTS");

        migrationBuilder.DropForeignKey(
            name: "FK_MESSAGES_SOURCES_SOURCE_ID",
            table: "MESSAGES");

        migrationBuilder.AlterColumn<long>(
            name: "SOURCE_ID",
            table: "MESSAGES",
            type: "LONG(20)",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "LONG(20)");

        migrationBuilder.AlterColumn<long>(
            name: "SOURCE_ID",
            table: "DOCUMENTS",
            type: "LONG(20)",
            nullable: true,
            oldClrType: typeof(long),
            oldType: "LONG(20)");

        migrationBuilder.AddForeignKey(
            name: "FK_DOCUMENTS_SOURCES_SOURCE_ID",
            table: "DOCUMENTS",
            column: "SOURCE_ID",
            principalTable: "SOURCES",
            principalColumn: "ID");

        migrationBuilder.AddForeignKey(
            name: "FK_MESSAGES_SOURCES_SOURCE_ID",
            table: "MESSAGES",
            column: "SOURCE_ID",
            principalTable: "SOURCES",
            principalColumn: "ID");
    }
}
