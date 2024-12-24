using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyShop.Migrations
{
    /// <inheritdoc />
    public partial class DelWishListAndThongKe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThongKes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThongKes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoTruyCap = table.Column<long>(type: "bigint", nullable: false),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongKes", x => x.Id);
                });
        }
    }
}
