using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Migrations
{
    public partial class AddVariantPricing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Gia",
                table: "KichThuocSanPhams",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaGiam",
                table: "KichThuocSanPhams",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTieuChuan",
                table: "KichThuocSanPhams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gia",
                table: "KichThuocSanPhams");

            migrationBuilder.DropColumn(
                name: "GiaGiam",
                table: "KichThuocSanPhams");

            migrationBuilder.DropColumn(
                name: "IsTieuChuan",
                table: "KichThuocSanPhams");
        }
    }
}
