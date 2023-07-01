using System;

namespace Paillave.EntityFrameworkCoreExtension.Searcher
{
    /// <summary>
    /// Permits to make a dictionary on a nullable key
    /// </summary>
    public struct GroupingValue : IEquatable<GroupingValue>
    {
        public GroupingValue(object value = null) => Value = value;
        public object Value { get; }
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public override string ToString() => this.Value?.ToString() ?? "";
        public bool Equals(GroupingValue v) => v.Value == Value;
    }
}