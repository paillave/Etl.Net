using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class ClassificationStrategyOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClassificationStrategy",
                schema: "Pms",
                table: "Security",
                nullable: true,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClassificationStrategy",
                schema: "Pms",
                table: "Security",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
