using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogTutorial.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Author",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Author", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Category",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Category", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ExecutionLog",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                ExecutionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Message = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExecutionLog", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Post",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                AuthorId = table.Column<int>(type: "int", nullable: false),
                CategoryId = table.Column<int>(type: "int", nullable: true),
                Discriminator = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Text = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Post", x => x.Id);
                table.ForeignKey(
                    name: "FK_Post_Author_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "Author",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Post_Category_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Category",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Author_Email",
            table: "Author",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Category_Code",
            table: "Category",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Post_AuthorId_DateTime",
            table: "Post",
            columns: new[] { "AuthorId", "DateTime" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Post_CategoryId",
            table: "Post",
            column: "CategoryId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ExecutionLog");

        migrationBuilder.DropTable(
            name: "Post");

        migrationBuilder.DropTable(
            name: "Author");

        migrationBuilder.DropTable(
            name: "Category");
    }
}
