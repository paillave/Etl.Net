using System;
using FundProcess.Pms.Imports.StreamTypes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.XmlFile.Extensions;
using Paillave.Etl.TextFile.Extensions;
using Paillave.Etl.EntityFrameworkCore.Extensions;
using Paillave.Etl.StreamNodes;
using FundProcess.Pms.Imports.FileDefinitions;
using FundProcess.Pms.DataAccess.Enums;
using FundProcess.Pms.DataAccess;
using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Linq;
using FundProcess.Pms.Imports.StreamTypes.Config;
using System.Text.RegularExpressions;
using FundProcess.Pms.Imports.StreamTypes.InputOutput;

namespace FundProcess.Pms.Imports.Jobs
{
    public class BdlJobs
    {
        public static void FullInitialImportProcessWithNoCtx(ISingleStream<BdlImportFilesConfigNoCtx> configStream)
        {
            FullInitialImport(configStream.Select("create config with Ctx",
                c => new BdlImportFilesConfigCtx
                {
                    InputFilesRootFolderPath = c.InputFilesRootFolderPath,
                    FileNamePattern = c.FileNamePattern,
                    DbContext = new DatabaseContext(
                        new DbContextOptionsBuilder<DatabaseContext>().UseSqlServer(new SqlConnectionStringBuilder
                        {
                            IntegratedSecurity = true,
                            DataSource = c.Server,
                            InitialCatalog = c.Database,
                            MultipleActiveResultSets = true
                        }.ConnectionString).Options,
                        new TenantContext(c.EntityId, c.EntityGroupId))
                }));
        }
        public static void FullInitialImport(ISingleStream<BdlImportFilesConfigCtx> configStream)
        {
            var dbCnxStream = configStream
                .Select("get dbcnx", i => i.DbContext, true);

            var nodesStream = configStream
                .CrossApplyFolderFiles("get all Nav files", i => i.InputFilesRootFolderPath, i => i.FileNamePattern, true)
                .CrossApplyXmlFile("parse input file", new BdlFileDefinition());

            var securitiesStream = nodesStream
                .Where("list securities", i => i.NodeDefinitionName == "secbase")
                .Select("cast to security", i => (BdlSecBaseNode)i.Value)
                .Distinct("distinct securities", i => i.SecurityCode);

            var securityPositionsStream = nodesStream
                .Where("list securities positions", i => i.NodeDefinitionName == "secpos")
                .Select("cast to security position", i => (BdlSecPosNode)i.Value);

            var managedSecurityCodes = securityPositionsStream
                .Distinct("distinct portfolio code from securities positions", i => i.ContId)
                .Select("get managed security code only", i => new { i.ContId });

            var managedSecurities = securitiesStream
                .Lookup("get matching security code from positions", managedSecurityCodes, i => i.SecurityCode, i => i.ContId, (l, r) => new { Security = l, IsManaged = r != null })
                .Where("keep managed securities", i => i.IsManaged);

            var cashPositionsStream = nodesStream
                .Where("list cash positions", i => i.NodeDefinitionName == "cashpos")
                .Select("cast to cash position", i => (BdlCashposNode)i.Value);
        }

        private static Regex _srriRegex = new Regex(@".+[^(][(](?<code>\d{1,2})[)]$", RegexOptions.Compiled);
        private int? ConvertSrri(string srriText)
        {
            var match = _srriRegex.Match(srriText);
            if (match == null) return null;
            var code = match.Groups["code"].Value;
            if (int.TryParse(code, out var srri)) return srri;
            else return null;
        }
    }
}
