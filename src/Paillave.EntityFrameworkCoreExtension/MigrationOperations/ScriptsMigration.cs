using Microsoft.EntityFrameworkCore.Migrations;

namespace Paillave.EntityFrameworkCoreExtension.MigrationOperations;

public class ScriptsMigration(string rootPath, params string[] scripts) : Migration
{
    private readonly string[] _scripts = scripts;
    private readonly string _rootPath = rootPath;

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.FromScripts(OperationType.Create, _rootPath, _scripts);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.FromScripts(OperationType.Drop, _rootPath, _scripts);
    }
}