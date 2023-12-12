using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicTacToeGame.WebUI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBoardFromDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Board",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "BoardTable",
                table: "Games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Board",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BoardTable",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
