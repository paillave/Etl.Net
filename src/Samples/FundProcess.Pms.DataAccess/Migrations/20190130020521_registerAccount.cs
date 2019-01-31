using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class registerAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ShareHolderId",
                schema: "Pms",
                table: "RegisterAccount",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "DistributorId",
                schema: "Pms",
                table: "RegisterAccount",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ShareHolderId",
                schema: "Pms",
                table: "RegisterAccount",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DistributorId",
                schema: "Pms",
                table: "RegisterAccount",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
