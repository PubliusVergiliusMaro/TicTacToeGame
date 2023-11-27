using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicTacToeGame.WebUI.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerTypeToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerType",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerType",
                table: "AspNetUsers");
        }
    }
}
