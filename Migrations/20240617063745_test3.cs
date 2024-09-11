using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alzaheimer.Migrations
{
    /// <inheritdoc />
    public partial class test3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_ShiftId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ShiftId",
                table: "Appointments",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_ShiftId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ShiftId",
                table: "Appointments",
                column: "ShiftId",
                unique: true);
        }
    }
}
