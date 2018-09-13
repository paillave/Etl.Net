using System;

namespace Paillave.Etl
{
    public static class FieldMapper
    {
        public static T ToColumn<T>(string columnName, string format = null)
        {
            throw new NotSupportedException();
        }
        public static T ToColumn<T>(int ColumnIndex, string format = null)
        {
            throw new NotSupportedException();
        }
    }
}
