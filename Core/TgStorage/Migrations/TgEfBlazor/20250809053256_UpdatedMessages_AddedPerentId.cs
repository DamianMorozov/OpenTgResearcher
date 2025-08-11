#nullable disable

namespace TgStorage.Migrations.TgEfBlazor;

/// <inheritdoc />
public partial class UpdatedMessages_AddedPerentId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "ID",
            table: "MESSAGES",
            type: "INT",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "LONG(20)");

        migrationBuilder.AddColumn<int>(
            name: "PARENT_ID",
            table: "MESSAGES",
            type: "INT",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<int>(
            name: "MESSAGE_ID",
            table: "DOCUMENTS",
            type: "INT",
            nullable: false,
            oldClrType: typeof(long),
            oldType: "LONG(20)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PARENT_ID",
            table: "MESSAGES");

        migrationBuilder.AlterColumn<long>(
            name: "ID",
            table: "MESSAGES",
            type: "LONG(20)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INT");

        migrationBuilder.AlterColumn<long>(
            name: "MESSAGE_ID",
            table: "DOCUMENTS",
            type: "LONG(20)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INT");
    }
}
