using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FundProcess.Pms.DataAccess.Migrations
{
    public partial class AccountPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegisterPosition",
                schema: "Pms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RegisterAccountId = table.Column<int>(nullable: false),
                    ShareClassId = table.Column<int>(nullable: false),
                    NbShares = table.Column<decimal>(nullable: false),
                    HoldingDate = table.Column<DateTime>(nullable: false),
                    MarketValueInShareClassCcy = table.Column<decimal>(nullable: true),
                    MarketValueInSubFundCcy = table.Column<decimal>(nullable: false),
                    BelongsToEntityId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterPosition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisterPosition_RegisterAccount_RegisterAccountId",
                        column: x => x.RegisterAccountId,
                        principalSchema: "Pms",
                        principalTable: "RegisterAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegisterPosition_Security_ShareClassId",
                        column: x => x.ShareClassId,
                        principalSchema: "Pms",
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegisterPosition_RegisterAccountId",
                schema: "Pms",
                table: "RegisterPosition",
                column: "RegisterAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterPosition_ShareClassId",
                schema: "Pms",
                table: "RegisterPosition",
                column: "ShareClassId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterPosition_HoldingDate_ShareClassId_RegisterAccountId_BelongsToEntityId",
                schema: "Pms",
                table: "RegisterPosition",
                columns: new[] { "HoldingDate", "ShareClassId", "RegisterAccountId", "BelongsToEntityId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegisterPosition",
                schema: "Pms");
        }
    }
}
