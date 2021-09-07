using Microsoft.EntityFrameworkCore.Migrations;

namespace EtsClientData.Migrations
{
    public partial class changed_ReminderTime_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Notified",
                table: "ReminderTimes",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Notified",
                table: "ReminderTimes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");
        }
    }
}
