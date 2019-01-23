using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class InternalCodeOnSicav : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InternalCode",
                schema: "Pms",
                table: "Sicav",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalCode",
                schema: "Pms",
                table: "Sicav");
        }
    }
}
