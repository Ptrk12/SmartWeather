using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class stringconversionandremoveuserpropfromdevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_ApplicationUserId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_ApplicationUserId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Devices");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Devices",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Devices",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ApplicationUserId",
                table: "Devices",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_ApplicationUserId",
                table: "Devices",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
