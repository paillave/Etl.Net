using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class MoreContraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoricalValue",
                schema: "Pms",
                table: "HistoricalValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataProviderSecurity",
                schema: "Pms",
                table: "DataProviderSecurity");

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "HistoricalValue",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "DataProviderSecurity",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HistoricalValue",
                schema: "Pms",
                table: "HistoricalValue",
                columns: new[] { "Date", "SecurityId", "Type", "BelongsToEntityId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataProviderSecurity",
                schema: "Pms",
                table: "DataProviderSecurity",
                columns: new[] { "SecurityId", "DataProvider", "BelongsToEntityId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoricalValue",
                schema: "Pms",
                table: "HistoricalValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataProviderSecurity",
                schema: "Pms",
                table: "DataProviderSecurity");

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "HistoricalValue",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "DataProviderSecurity",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddPrimaryKey(
                name: "PK_HistoricalValue",
                schema: "Pms",
                table: "HistoricalValue",
                columns: new[] { "Date", "SecurityId", "Type" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataProviderSecurity",
                schema: "Pms",
                table: "DataProviderSecurity",
                columns: new[] { "SecurityId", "DataProvider" });
        }
    }
}
