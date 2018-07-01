using System.Globalization;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Helpers.MapperFactories
{
    public class PropertyDescription
    {
        public LambdaExpression MemberLamda { get; set; }
        public CultureInfo CultureInfo { get; set; }
    }
}
