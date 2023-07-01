using System;

namespace Paillave.Etl.SqlServer.Core
{
    public interface ISqlResultMapper
    {
        DateTime ToDateColumn(string columnName);
        DateTime? ToOptionalDateColumn(string columnName);
        T ToNumberColumn<T>(string columnName);
        T ToColumn<T>(string columnName);
        string ToColumn(string columnName);
        bool ToBooleanColumn(string columnName);
        bool? ToOptionalBooleanColumn(string columnName);


        DateTime ToDateColumn();
        DateTime? ToOptionalDateColumn();
        T ToNumberColumn<T>();
        T ToColumn<T>();
        string ToColumn();
        bool ToBooleanColumn();
        bool? ToOptionalBooleanColumn();
    }
}