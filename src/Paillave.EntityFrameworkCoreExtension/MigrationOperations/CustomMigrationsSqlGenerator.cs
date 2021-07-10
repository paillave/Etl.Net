using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.FileProviders;

namespace Paillave.EntityFrameworkCoreExtension.MigrationOperations
{
    public class CustomMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
        private Assembly _assembly = null;
        public CustomMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
        {
            this._assembly = dependencies.CurrentContext.Context.GetType().Assembly;
        }
        protected override void Generate(
                MigrationOperation operation,
                IModel model,
                MigrationCommandListBuilder builder)
        {
            switch (operation)
            {
                case FromScriptsMigrationOperation fromScriptsMigrationOperation:
                    Generate(fromScriptsMigrationOperation, builder);
                    break;
                default:
                    base.Generate(operation, model, builder);
                    break;
            }
        }

        protected void Generate(FromScriptsMigrationOperation fromScriptsMigrationOperation, MigrationCommandListBuilder builder)
        {
            Regex regex = new Regex(@"[\s\r]*create([\s\r]+or[\s\r]+alter)?([\s\r]+clustered)?([\s\r]+unique)?[\s\r]+(?<type>\w+)[\s\r]+(?<name>[^\s\r]+)[\s\r]+(?<body>.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            IFileProvider fileProvider = new ManifestEmbeddedFileProvider(this._assembly, fromScriptsMigrationOperation.RootPath);
            var scripts = (fromScriptsMigrationOperation.OperationType == OperationType.Create) ? fromScriptsMigrationOperation.Scripts.ToList() : fromScriptsMigrationOperation.Scripts.Reverse().ToList();
            foreach (var script in scripts)
            {
                var fileInfo = fileProvider.GetFileInfo($"{script}.sql");
                using (var sr = new StreamReader(fileInfo.CreateReadStream()))
                {
                    var fileContent = sr.ReadToEnd();//[^\s\r]
                    var match = regex.Match(fileContent);
                    if (match.Success)
                    {
                        Generate(fromScriptsMigrationOperation.OperationType, match.Groups["type"].Value, match.Groups["name"].Value, match.Groups["body"].Value, builder);
                    }
                }
            }
        }
        protected void Generate(OperationType operationType, string type, string name, string body, MigrationCommandListBuilder builder)
        {
            var sqlHelper = Dependencies.SqlGenerationHelper;
            switch (operationType)
            {
                case OperationType.Create:
                    builder.AppendLine($"create {type} {name}").AppendLine(body).EndCommand();
                    break;
                case OperationType.Drop:
                    builder.Append($"drop {type} {name}").AppendLine(sqlHelper.StatementTerminator).EndCommand();
                    break;
            }
        }
    }
}