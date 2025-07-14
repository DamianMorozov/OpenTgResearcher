#nullable disable

namespace TgStorage.Migrations.TgEfMemory;

/// <inheritdoc />
public partial class UpdatedMessages_AddedUserId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "USER_ID",
            table: "MESSAGES",
            type: "LONG(20)",
            nullable: false,
            defaultValue: 0L);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "USER_ID",
            table: "MESSAGES");
    }
}
