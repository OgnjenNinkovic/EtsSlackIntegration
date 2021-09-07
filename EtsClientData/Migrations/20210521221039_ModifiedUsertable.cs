using Microsoft.EntityFrameworkCore.Migrations;

namespace EtsClientData.Migrations
{
    public partial class ModifiedUsertable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeID",
                table: "Vacations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EtsPassword",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EtsUserName",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeID",
                table: "Vacations");

            migrationBuilder.DropColumn(
                name: "EtsPassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EtsUserName",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
