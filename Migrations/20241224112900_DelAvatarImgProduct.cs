using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyShop.Migrations
{
    /// <inheritdoc />
    public partial class DelAvatarImgProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "tb_Review");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "tb_Product");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "tb_Review",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "tb_Product",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
