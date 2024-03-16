using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivDocApp.Migrations
{
    /// <inheritdoc />
    public partial class fs5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocImg",
                table: "Doctors",
                newName: "DocImgURL");

            migrationBuilder.AddColumn<int>(
                name: "Experience",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Experience",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "DocImgURL",
                table: "Doctors",
                newName: "DocImg");
        }
    }
}
