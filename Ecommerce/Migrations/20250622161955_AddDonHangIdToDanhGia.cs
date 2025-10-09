using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddDonHangIdToDanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonHangId",
                table: "DanhGias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_DonHangId",
                table: "DanhGias",
                column: "DonHangId");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_DonHangs_DonHangId",
                table: "DanhGias",
                column: "DonHangId",
                principalTable: "DonHangs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_DonHangs_DonHangId",
                table: "DanhGias");

            migrationBuilder.DropIndex(
                name: "IX_DanhGias_DonHangId",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "DonHangId",
                table: "DanhGias");
        }
    }
}
