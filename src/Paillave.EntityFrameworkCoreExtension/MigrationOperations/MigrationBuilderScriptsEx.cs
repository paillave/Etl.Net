using System.Buffers.Text;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Paillave.EntityFrameworkCoreExtension.MigrationOperations;

public static class MigrationBuilderScriptsEx
{
    public static MigrationBuilder FromScripts(this MigrationBuilder migrationBuilder, OperationType operationType, string rootPath, params string[] scripts)
    {
        migrationBuilder.Operations.Add(new FromScriptsMigrationOperation
        {
            RootPath = rootPath,
            Scripts = scripts,
            OperationType = operationType
        });
        return migrationBuilder;
    }

    // public static MigrationBuilder CreateView(this MigrationBuilder migrationBuilder, string name, string selectStatement)
    // {
    //     migrationBuilder.Operations.Add(new CreateViewMigrationOperation
    //     {
    //         SelectStatement = selectStatement,
    //         Name = name,
    //     });
    //     return migrationBuilder;
    // }
    // public static MigrationBuilder CreateView(this MigrationBuilder migrationBuilder, string schema, string name, string selectStatement)
    // {
    //     migrationBuilder.Operations.Add(new CreateViewMigrationOperation
    //     {
    //         SelectStatement = selectStatement,
    //         Name = name,
    //         Schema = schema
    //     });
    //     return migrationBuilder;
    // }
    // public static MigrationBuilder DropView(this MigrationBuilder migrationBuilder, string name)
    // {
    //     migrationBuilder.Operations.Add(new DropViewMigrationOperation
    //     {
    //         Name = name,
    //     });
    //     return migrationBuilder;
    // }
    // public static MigrationBuilder DropView(this MigrationBuilder migrationBuilder, string schema, string name)
    // {
    //     migrationBuilder.Operations.Add(new DropViewMigrationOperation
    //     {
    //         Name = name,
    //         Schema = schema
    //     });
    //     return migrationBuilder;
    // }
}