using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EFCore.BulkExtensions
{
    public static class SqlQueryBuilder
    {
        public static string CreateTableCopy(string existingTableName, string newTableName, TableInfo tableInfo)
        {
            // TODO: (optionaly) if CalculateStats = True but SetOutputIdentity = False then Columns could be ommited from Create and from MergeOutput
            List<string> columnsNames = tableInfo.PropertyColumnNamesDict.Values.ToList();
            if (tableInfo.TimeStampColumnName != null)
            {
                columnsNames.Remove(tableInfo.TimeStampColumnName);
            }

            var q = $@"SELECT TOP 0 {GetCommaSeparatedColumns(columnsNames, "T")}, 0 as TempColumnNumOrder
                    INTO {newTableName} FROM {existingTableName} AS T 
                    LEFT JOIN {existingTableName} AS Source ON 1 = 0;"; // removes Identity constrain
            return q;
        }

        public static string DropTable(string tableName)
        {
            var q = $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP TABLE {tableName}";
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
            string outputTable = $"{tableInfo.FullTempTableName}Output";
            bool keepIdentity = tableInfo.BulkConfig.SqlBulkCopyOptions.HasFlag(SqlBulkCopyOptions.KeepIdentity);
            List<string> primaryKeys = tableInfo.PivotKeys.Select(k => tableInfo.PropertyColumnNamesDict[k]).ToList();
            List<string> columnsNames = tableInfo.PropertyColumnNamesDict.Values.ToList();
            List<string> outputColumnsNames = tableInfo.OutputPropertyColumnNamesDict.Values.ToList();
            List<string> nonPivotColumnsNames = columnsNames.Where(a => !primaryKeys.Contains(a)).ToList();
            List<string> nonPivotAndNotIdentityColumnsNames = nonPivotColumnsNames.Where(a => tableInfo.IdentityColumn != a).ToList();
            List<string> nonIdentityColumnsNames = columnsNames.Where(a => tableInfo.IdentityColumn != a).ToList();
            List<string> insertColumnsNames = (tableInfo.HasIdentityColumn && !keepIdentity) ? nonIdentityColumnsNames : columnsNames;
            List<string> updateColumnsNames = (tableInfo.HasIdentityColumn && !keepIdentity) ? nonPivotAndNotIdentityColumnsNames : nonPivotColumnsNames;

            var q = $@"MERGE {targetTable} WITH (HOLDLOCK) AS T 
                    USING {sourceTable} AS S 
                    ON {GetANDSeparatedColumns(primaryKeys, "T", "S", tableInfo.UpdateByPropertiesAreNullable)}
                    WHEN NOT MATCHED BY TARGET THEN INSERT ({GetCommaSeparatedColumns(insertColumnsNames)})
                    VALUES ({GetCommaSeparatedColumns(insertColumnsNames, "S")})
                    WHEN MATCHED THEN UPDATE SET {GetCommaSeparatedColumns(updateColumnsNames, "T", "S")}
                    OUTPUT {GetCommaSeparatedColumns(outputColumnsNames, "INSERTED")}, S.TempColumnNumOrder
                    INTO {outputTable};";
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
