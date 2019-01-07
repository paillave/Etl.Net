using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class OptionalLinksFromSecurity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MarketPlaceId",
                schema: "Pms",
                table: "Security",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "IcbSectorId",
                schema: "Pms",
                table: "Security",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "GicsSectorId",
                schema: "Pms",
                table: "Security",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "BenchmarkId",
                schema: "Pms",
                table: "Security",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MarketPlaceId",
                schema: "Pms",
                table: "Security",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IcbSectorId",
                schema: "Pms",
                table: "Security",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GicsSectorId",
                schema: "Pms",
                table: "Security",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BenchmarkId",
                schema: "Pms",
                table: "Security",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
