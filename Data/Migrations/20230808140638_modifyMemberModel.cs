using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniLibrary.Data.Migrations
{
    public partial class modifyMemberModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Members",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "Members",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "Members");
        }
    }
}
