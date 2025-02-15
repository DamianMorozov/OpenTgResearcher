#nullable disable

namespace TgStorage.Migrations.TgEfBlazor
{
    /// <inheritdoc />
    public partial class UpdatedApps_AddedBotToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BOT_TOKEN",
                table: "APPS",
                type: "NVARCHAR(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IS_BOT",
                table: "APPS",
                type: "BIT",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BOT_TOKEN",
                table: "APPS");

            migrationBuilder.DropColumn(
                name: "IS_BOT",
                table: "APPS");
        }
    }
}
