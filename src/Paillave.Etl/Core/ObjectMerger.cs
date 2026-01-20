using System.Linq;

namespace Paillave.Etl.Core;

public class ObjectMerger
{
    public static T MergeNotNull<T>(T targetSource, T source)
    {
        if (targetSource == null)
            return source;
        var properties = typeof(T).GetProperties();
        var builder = new ObjectBuilder<T>();
        properties
            .Select(i => new { PropertyName = i.Name, TargetValue = i.GetValue(targetSource), SourceValue = i.GetValue(source) })
            .ToList()
            .ForEach(i => builder.Values[i.PropertyName] = i.TargetValue ?? i.SourceValue);
        return builder.CreateInstance();
    }
}