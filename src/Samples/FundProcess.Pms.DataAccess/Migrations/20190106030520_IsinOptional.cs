using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class IsinOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Isin",
                schema: "Pms",
                table: "Security",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 12);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Isin",
                schema: "Pms",
                table: "Security",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 12,
                oldNullable: true);
        }
    }
}
