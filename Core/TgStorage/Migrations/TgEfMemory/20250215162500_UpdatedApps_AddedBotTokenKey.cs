#nullable disable

namespace TgStorage.Migrations.TgEfMemory
{
    /// <inheritdoc />
    public partial class UpdatedApps_AddedBotTokenKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BOT_TOKEN_KEY",
                table: "APPS",
                type: "NVARCHAR(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "USE_BOT",
                table: "APPS",
                type: "BIT",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BOT_TOKEN_KEY",
                table: "APPS");

            migrationBuilder.DropColumn(
                name: "USE_BOT",
                table: "APPS");
        }
    }
}
