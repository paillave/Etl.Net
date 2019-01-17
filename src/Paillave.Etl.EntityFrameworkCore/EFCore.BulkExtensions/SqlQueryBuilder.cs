using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EFCore.BulkExtensions
{
    public static class SqlQueryBuilder
    {
        public static string CreateTableCopy(string existingTableName, string newTableName, TableInfo tableInfo, bool isOutputTable = false)
        {
            // TODO: (optionaly) if CalculateStats = True but SetOutputIdentity = False then Columns could be ommited from Create and from MergeOutput
            List<string> columnsNames = (isOutputTable ? tableInfo.OutputPropertyColumnNamesDict : tableInfo.PropertyColumnNamesDict).Values.ToList();
            if (tableInfo.TimeStampColumnName != null)
            {
                columnsNames.Remove(tableInfo.TimeStampColumnName);
            }

            var q = $"SELECT TOP 0 {GetCommaSeparatedColumns(columnsNames, "T")} " +
                    $"INTO {newTableName} FROM {existingTableName} AS T " +
                    $"LEFT JOIN {existingTableName} AS Source ON 1 = 0;"; // removes Identity constrain
            return q;
        }

        public static string AddColumn(string fullTableName, string columnName, string columnType)
        {
            var q = $"ALTER TABLE {fullTableName} ADD [{columnName}] {columnType};";
            return q;
        }

        public static string SelectFromOutputTable(TableInfo tableInfo)
        {
            List<string> columnsNames = tableInfo.OutputPropertyColumnNamesDict.Values.ToList();
            var q = $"SELECT {GetCommaSeparatedColumns(columnsNames)} FROM {tableInfo.FullTempOutputTableName}";
            return q;
        }

        public static string SelectCountIsUpdateFromOutputTable(TableInfo tableInfo)
        {
            var q = $"SELECT COUNT(*) FROM {tableInfo.FullTempOutputTableName} WHERE [IsUpdate] = 1";
            return q;
        }

        public static string DropTable(string tableName)
        {
            var q = $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP TABLE {tableName}";
            return q;
        }

        public static string SelectJoinTable(TableInfo tableInfo)
        {
            string sourceTable = tableInfo.FullTableName;
            string joinTable = tableInfo.FullTempTableName;
            List<string> columnsNames = tableInfo.PropertyColumnNamesDict.Values.ToList();
            List<string> selectByPropertyNames = tableInfo.PropertyColumnNamesDict.Where(a => tableInfo.PivotKeys.Contains(a.Key)).Select(a => a.Value).ToList();

            var q = $"SELECT {GetCommaSeparatedColumns(columnsNames, "S")} FROM {sourceTable} AS S " +
                    $"JOIN {joinTable} AS J " +
                    $"ON {GetANDSeparatedColumns(selectByPropertyNames, "S", "J", tableInfo.UpdateByPropertiesAreNullable)}" +
                    $";";
            return q;
        }

        public static string SetIdentityInsert(string tableName, bool identityInsert)
        {
            string ON_OFF = identityInsert ? "ON" : "OFF";
            var q = $"SET IDENTITY_INSERT {tableName} {ON_OFF};";
            return q;
        }

        public static string MergeTable(TableInfo tableInfo)
        {
            string targetTable = tableInfo.FullTableName;
            string sourceTable = tableInfo.FullTempTableName;
            bool keepIdentity = tableInfo.BulkConfig.SqlBulkCopyOptions.HasFlag(SqlBulkCopyOptions.KeepIdentity);
            List<string> primaryKeys = tableInfo.PivotKeys.Select(k => tableInfo.PropertyColumnNamesDict[k]).ToList();
            List<string> columnsNames = tableInfo.PropertyColumnNamesDict.Values.ToList();
            List<string> outputColumnsNames = tableInfo.OutputPropertyColumnNamesDict.Values.ToList();
            List<string> nonPivotColumnsNames = columnsNames.Where(a => !primaryKeys.Contains(a)).ToList();
            List<string> nonPivotAndNotIdentityColumnsNames = nonPivotColumnsNames.Where(a => tableInfo.IdentityColumn != a).ToList();
            List<string> nonIdentityColumnsNames = columnsNames.Where(a => tableInfo.IdentityColumn != a).ToList();
            List<string> insertColumnsNames = (tableInfo.HasIdentityColumn && !keepIdentity) ? nonIdentityColumnsNames : columnsNames;
            List<string> updateColumnsNames = (tableInfo.HasIdentityColumn && !keepIdentity) ? nonPivotAndNotIdentityColumnsNames : nonPivotColumnsNames;

            string textWITH_HOLDLOCK = tableInfo.BulkConfig.WithHoldlock ? " WITH (HOLDLOCK)" : "";

            var q = $"MERGE {targetTable}{textWITH_HOLDLOCK} AS T " +
                    $"USING {sourceTable} AS S " +
                    $"ON {GetANDSeparatedColumns(primaryKeys, "T", "S", tableInfo.UpdateByPropertiesAreNullable)}";

            q += $" WHEN NOT MATCHED BY TARGET THEN INSERT ({GetCommaSeparatedColumns(insertColumnsNames)})" +
                 $" VALUES ({GetCommaSeparatedColumns(insertColumnsNames, "S")})";
            q += $" WHEN MATCHED THEN UPDATE SET {GetCommaSeparatedColumns(updateColumnsNames, "T", "S")}";
            q += $" OUTPUT {GetCommaSeparatedColumns(outputColumnsNames, "INSERTED")}";
            q += ";";
            return q;
        }

        public static string GetCommaSeparatedColumns(List<string> columnsNames, string prefixTable = null, string equalsTable = null)
        {
            string commaSeparatedColumns = "";
            foreach (var columnName in columnsNames)
            {
                commaSeparatedColumns += prefixTable != null ? $"{prefixTable}.[{columnName}]" : $"[{columnName}]";
                commaSeparatedColumns += equalsTable != null ? $" = {equalsTable}.[{columnName}]" : "";
                commaSeparatedColumns += ", ";
            }
            if (commaSeparatedColumns != "")
            {
                commaSeparatedColumns = commaSeparatedColumns.Remove(commaSeparatedColumns.Length - 2, 2); // removes last excess comma and space: ", "
            }
            return commaSeparatedColumns;
        }

        public static string GetANDSeparatedColumns(List<string> columnsNames, string prefixTable = null, string equalsTable = null, bool updateByPropertiesAreNullable = false)
        {
            string commaSeparatedColumns = GetCommaSeparatedColumns(columnsNames, prefixTable, equalsTable);

            if (updateByPropertiesAreNullable)
            {
                string[] columns = commaSeparatedColumns.Split(',');
                string commaSeparatedColumnsNullable = String.Empty;
                foreach (var column in columns)
                {
                    string[] columnTS = column.Split('=');
                    string columnT = columnTS[0].Trim();
                    string columnS = columnTS[1].Trim();
                    string columnNullable = $"({column.Trim()} OR ({columnT} IS NULL AND {columnS} IS NULL))";
                    commaSeparatedColumnsNullable += columnNullable + ", ";
                }
                if (commaSeparatedColumns != "")
                {
                    commaSeparatedColumnsNullable = commaSeparatedColumnsNullable.Remove(commaSeparatedColumnsNullable.Length - 2, 2);
                }
                commaSeparatedColumns = commaSeparatedColumnsNullable;
            }

            string ANDSeparatedColumns = commaSeparatedColumns.Replace(",", " AND");
            return ANDSeparatedColumns;
        }
    }
}
