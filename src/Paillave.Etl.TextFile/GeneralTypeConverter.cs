using System;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;

namespace Paillave.Etl.TextFile;

public class GeneralTypeConverter : TypeConverter
{
    private readonly TypeConverter _innerTypeConverter;
    private TypeConverter GetConverter(Type type, string[] trueValues, string[] falseValues)
    {
        if (type == typeof(System.Byte) || type == typeof(System.Byte?)) return new ByteTypeConverter();
        if (type == typeof(System.SByte) || type == typeof(System.SByte?)) return new SByteTypeConverter();
        if (type == typeof(System.Decimal) || type == typeof(System.Decimal?)) return new DecimalTypeConverter();
        if (type == typeof(System.Double) || type == typeof(System.Double?)) return new DoubleTypeConverter();
        if (type == typeof(System.Single) || type == typeof(System.Single?)) return new SingleTypeConverter();
        if (type == typeof(System.Int32) || type == typeof(System.Int32?)) return new Int32TypeConverter();
        if (type == typeof(System.UInt32) || type == typeof(System.UInt32?)) return new UInt32TypeConverter();
        if (type == typeof(System.Int64) || type == typeof(System.Int64?)) return new Int64TypeConverter();
        if (type == typeof(System.UInt64) || type == typeof(System.UInt64?)) return new UInt64TypeConverter();
        if (type == typeof(System.Int16) || type == typeof(System.Int16?)) return new Int16TypeConverter();
        if (type == typeof(System.UInt16) || type == typeof(System.UInt16?)) return new UInt16TypeConverter();
        if (type == typeof(System.DateTime) || type == typeof(System.DateTime?)) return new DateTimeTypeConverter();
        if (type == typeof(System.Boolean) || type == typeof(System.Boolean?)) return new BooleanTypeConverter(trueValues, falseValues);
        return TypeDescriptor.GetConverter(type);
    }
    public GeneralTypeConverter(Type targetType, string[] trueValues, string[] falseValues)
    {
        _innerTypeConverter = this.GetConverter(targetType, trueValues, falseValues);
    }
    public GeneralTypeConverter(Type targetType)
    {
        _innerTypeConverter = this.GetConverter(targetType, new string[] { }, new string[] { });
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
        => _innerTypeConverter.ConvertFrom(context, cultureInfo, value);
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => _innerTypeConverter.ConvertTo(context, culture, value, destinationType);
}
internal class ByteTypeConverter : TypeConverter
{
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.Byte.Parse(text, cultureInfo.NumberFormat);
    }
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.Byte)value;
        return v.ToString(culture);
    }
}
internal class SByteTypeConverter : TypeConverter
{
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.SByte.Parse(text, cultureInfo.NumberFormat);
    }
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.SByte)value;
        return v.ToString(culture);
    }
}
public class DecimalTypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.Decimal)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.Decimal.Parse(text, cultureInfo.NumberFormat);
    }
}
internal class DoubleTypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.Double)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.Double.Parse(text, cultureInfo.NumberFormat);
    }
}
internal class SingleTypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.Single)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.Single.Parse(text, cultureInfo.NumberFormat);
    }
}
internal class Int32TypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.Int32)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.Int32.Parse(text, cultureInfo.NumberFormat);
    }
}
internal class UInt32TypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.UInt32)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.UInt32.Parse(text, cultureInfo.NumberFormat);
    }
}
internal class Int64TypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.Int64)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.Int64.Parse(text, cultureInfo.NumberFormat);
    }
}
internal class UInt64TypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.UInt64)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.UInt64.Parse(text, cultureInfo.NumberFormat);
    }
}
internal class Int16TypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.Int16)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.Int16.Parse(text, cultureInfo.NumberFormat);
    }
}
internal class UInt16TypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.UInt16)value;
        return v.ToString(culture);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return System.UInt16.Parse(text, cultureInfo.NumberFormat);
    }
}

internal class BooleanTypeConverter(string[] trueValues, string[] falseValues) : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.Boolean)value;
        if (_falseValue != null && !v) return _falseValue;
        if (_trueValue != null && v) return _trueValue;
        return v.ToString(culture);
    }
    private readonly TypeConverter _innerTypeConverter = TypeDescriptor.GetConverter(typeof(bool));
    private readonly string _trueValue = trueValues.FirstOrDefault();
    private readonly string _falseValue = falseValues.FirstOrDefault();
    private readonly HashSet<string> _trueValues = trueValues.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
    private readonly HashSet<string> _falseValues = falseValues.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (_trueValues.Contains(text.Trim())) return true;
        if (_falseValues.Contains(text.Trim())) return false;
        return _innerTypeConverter.ConvertFromString(text);
    }
}
internal class DateTimeTypeConverter : TypeConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (value == null) return "";
        var v = (System.DateTime)value;
        return v.ToString(culture.DateTimeFormat.LongDatePattern);
    }
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text)) return null;
        return DateTime.ParseExact(text.Trim(), culture.DateTimeFormat.LongDatePattern, culture);
    }
}