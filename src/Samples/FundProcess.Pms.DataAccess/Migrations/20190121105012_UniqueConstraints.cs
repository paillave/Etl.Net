using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class UniqueConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "Position",
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

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Security_BelongsToEntityId_InternalCode",
                schema: "Pms",
                table: "Security",
                columns: new[] { "BelongsToEntityId", "InternalCode" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Position_BelongsToEntityId_PortfolioCompositionId_SecurityId",
                schema: "Pms",
                table: "Position",
                columns: new[] { "BelongsToEntityId", "PortfolioCompositionId", "SecurityId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PortfolioComposition_BelongsToEntityId_Date_PortfolioId",
                schema: "Pms",
                table: "PortfolioComposition",
                columns: new[] { "BelongsToEntityId", "Date", "PortfolioId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Security_BelongsToEntityId_InternalCode",
                schema: "Pms",
                table: "Security");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Position_BelongsToEntityId_PortfolioCompositionId_SecurityId",
                schema: "Pms",
                table: "Position");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PortfolioComposition_BelongsToEntityId_Date_PortfolioId",
                schema: "Pms",
                table: "PortfolioComposition");

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

            migrationBuilder.AlterColumn<int>(
                name: "BelongsToEntityId",
                schema: "Pms",
                table: "Position",
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
        }
    }
}
