#nullable disable

namespace TgStorage.Migrations.TgEfTest;

/// <inheritdoc />
public partial class UpdatedMessages_AddedIsDeleted : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IS_DELETED",
            table: "MESSAGES",
            type: "BIT",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IS_DELETED",
            table: "MESSAGES");
    }
}
