using Microsoft.EntityFrameworkCore.Migrations;

namespace Paillave.EntityFrameworkCoreExtension.MigrationOperations
{
    public class ScriptsMigration : Migration
    {
        private string[] _scripts = new string[] { };
        private string _rootPath = null;
        public ScriptsMigration(string rootPath, params string[] scripts)
        {
            _scripts = scripts;
            _rootPath = rootPath;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.FromScripts(OperationType.Create, _rootPath, _scripts);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.FromScripts(OperationType.Drop, _rootPath, _scripts);
        }
    }
}