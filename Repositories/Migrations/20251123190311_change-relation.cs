using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class changerelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alerts_SensorMetricId",
                table: "Alerts");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SensorMetricId",
                table: "Alerts",
                column: "SensorMetricId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alerts_SensorMetricId",
                table: "Alerts");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SensorMetricId",
                table: "Alerts",
                column: "SensorMetricId");
        }
    }
}
