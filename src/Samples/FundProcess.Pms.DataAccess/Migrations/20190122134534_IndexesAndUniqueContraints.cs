using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class IndexesAndUniqueContraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Position_PortfolioCompositionId",
                schema: "Pms",
                table: "Position");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoricalValue",
                schema: "Pms",
                table: "HistoricalValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataProviderSecurity",
                schema: "Pms",
                table: "DataProviderSecurity");

            migrationBuilder.RenameColumn(
                name: "SicavName",
                schema: "Pms",
                table: "Sicav",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "Security",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "Pms",
                table: "PortfolioComposition",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "PortfolioComposition",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

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

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Security_BelongsToEntityId_InternalCode",
                schema: "Pms",
                table: "Security",
                columns: new[] { "BelongsToEntityId", "InternalCode" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PortfolioComposition_Date_PortfolioId_BelongsToEntityId",
                schema: "Pms",
                table: "PortfolioComposition",
                columns: new[] { "Date", "PortfolioId", "BelongsToEntityId" });

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

            migrationBuilder.CreateIndex(
                name: "IX_Position_PortfolioCompositionId_SecurityId_BelongsToEntityId",
                schema: "Pms",
                table: "Position",
                columns: new[] { "PortfolioCompositionId", "SecurityId", "BelongsToEntityId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Security_BelongsToEntityId_InternalCode",
                schema: "Pms",
                table: "Security");

            migrationBuilder.DropIndex(
                name: "IX_Position_PortfolioCompositionId_SecurityId_BelongsToEntityId",
                schema: "Pms",
                table: "Position");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PortfolioComposition_Date_PortfolioId_BelongsToEntityId",
                schema: "Pms",
                table: "PortfolioComposition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoricalValue",
                schema: "Pms",
                table: "HistoricalValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataProviderSecurity",
                schema: "Pms",
                table: "DataProviderSecurity");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Pms",
                table: "Sicav",
                newName: "SicavName");

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "Security",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "Pms",
                table: "PortfolioComposition",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "PortfolioComposition",
                nullable: true,
                oldClrType: typeof(int));

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

            migrationBuilder.CreateIndex(
                name: "IX_Position_PortfolioCompositionId",
                schema: "Pms",
                table: "Position",
                column: "PortfolioCompositionId");
        }
    }
}
