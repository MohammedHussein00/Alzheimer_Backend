using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alzaheimer.Migrations
{
    /// <inheritdoc />
    public partial class test4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PatientId",
                table: "Detections",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Detections_PatientId",
                table: "Detections",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Detections_AspNetUsers_PatientId",
                table: "Detections",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detections_AspNetUsers_PatientId",
                table: "Detections");

            migrationBuilder.DropIndex(
                name: "IX_Detections_PatientId",
                table: "Detections");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Detections");
        }
    }
}
