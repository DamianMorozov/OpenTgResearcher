#nullable disable

namespace TgStorage.Migrations.TgEfDesktop
{
    /// <inheritdoc />
    public partial class UpdatedApps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PHONE_NUMBER",
                table: "APPS",
                type: "NVARCHAR(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "CHAR(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "FIRST_NAME",
                table: "APPS",
                type: "NVARCHAR(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LAST_NAME",
                table: "APPS",
                type: "NVARCHAR(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FIRST_NAME",
                table: "APPS");

            migrationBuilder.DropColumn(
                name: "LAST_NAME",
                table: "APPS");

            migrationBuilder.AlterColumn<string>(
                name: "PHONE_NUMBER",
                table: "APPS",
                type: "CHAR(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(20)",
                oldMaxLength: 20);
        }
    }
}
