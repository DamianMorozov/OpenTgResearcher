#nullable disable

namespace TgStorage.Migrations.TgEfTest;

/// <inheritdoc />
public partial class UpdatedMessages_UpdatedUserId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_MESSAGES_USER_ID",
            table: "MESSAGES",
            column: "USER_ID");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_MESSAGES_USER_ID",
            table: "MESSAGES");
    }
}
