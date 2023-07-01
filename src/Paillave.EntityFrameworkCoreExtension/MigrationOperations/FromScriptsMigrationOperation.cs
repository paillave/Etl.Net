using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Paillave.EntityFrameworkCoreExtension.MigrationOperations
{
    public class FromScriptsMigrationOperation : MigrationOperation
    {
        public string RootPath { get; set; }
        public string[] Scripts { get; set; }
        public OperationType OperationType { get; set; }
        public override bool IsDestructiveChange { get => false; set { } }
    }
    public enum OperationType
    {
        Create = 1,
        Drop = 2,
    }
}