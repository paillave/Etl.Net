using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Paillave.Etl.Samples.Migrations
{
    public partial class TestDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Security",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternalCode = table.Column<string>(nullable: false),
                    Isin = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    Issuer = table.Column<string>(nullable: true),
                    Class = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security", x => x.Id);
                    table.UniqueConstraint("AK_Security_InternalCode", x => x.InternalCode);
                });

            migrationBuilder.CreateTable(
                name: "Sicav",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternalCode = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sicav", x => x.Id);
                    table.UniqueConstraint("AK_Sicav_InternalCode", x => x.InternalCode);
                });

            migrationBuilder.CreateTable(
                name: "Portfolio",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternalCode = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SicavId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolio", x => x.Id);
                    table.UniqueConstraint("AK_Portfolio_InternalCode", x => x.InternalCode);
                    table.ForeignKey(
                        name: "FK_Portfolio_Sicav_SicavId",
                        column: x => x.SicavId,
                        principalTable: "Sicav",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Composition",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "DATE", nullable: false),
                    PortfolioId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Composition", x => x.Id);
                    table.UniqueConstraint("AK_Composition_Date_PortfolioId", x => new { x.Date, x.PortfolioId });
                    table.ForeignKey(
                        name: "FK_Composition_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                columns: table => new
                {
                    CompositionId = table.Column<int>(nullable: false),
                    SecurityId = table.Column<int>(nullable: false),
                    Value = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => new { x.CompositionId, x.SecurityId });
                    table.ForeignKey(
                        name: "FK_Position_Composition_CompositionId",
                        column: x => x.CompositionId,
                        principalTable: "Composition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Position_Security_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Security",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Composition_PortfolioId",
                table: "Composition",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolio_SicavId",
                table: "Portfolio",
                column: "SicavId");

            migrationBuilder.CreateIndex(
                name: "IX_Position_SecurityId",
                table: "Position",
                column: "SecurityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Position");

            migrationBuilder.DropTable(
                name: "Composition");

            migrationBuilder.DropTable(
                name: "Security");

            migrationBuilder.DropTable(
                name: "Portfolio");

            migrationBuilder.DropTable(
                name: "Sicav");
        }
    }
}
