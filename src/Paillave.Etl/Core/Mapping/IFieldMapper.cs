namespace Paillave.Etl.Core.Mapping
{
    public interface IFieldMapper
    {
        T ToDateColumn<T>(int columnIndex, string dateFormat);
        T ToCulturedDateColumn<T>(int columnIndex, string cultureName);
        T ToCulturedDateColumn<T>(int columnIndex, string cultureName, string dateFormat);
        T ToNumberColumn<T>(int columnIndex, string decimalSeparator);
        T ToColumn<T>(int columnIndex);

        T ToDateColumn<T>(int columnIndex, int size, string dateFormat);
        T ToCulturedDateColumn<T>(int columnIndex, int size, string cultureName);
        T ToCulturedDateColumn<T>(int columnIndex, int size, string cultureName, string dateFormat);
        T ToNumberColumn<T>(int columnIndex, int size, string decimalSeparator);
        T ToColumn<T>(int columnIndex, int size);


        T ToDateColumn<T>(string columnName, string dateFormat);
        T ToCulturedDateColumn<T>(string columnName, string cultureName);
        T ToCulturedDateColumn<T>(string columnName, string cultureName, string dateFormat);
        T ToNumberColumn<T>(string columnName, string decimalSeparator);
        T ToColumn<T>(string columnName);
    }
}