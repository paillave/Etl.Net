using System.Linq.Expressions;

namespace Paillave.Etl.Core.Mapping
{
    public class MappingProcessor<T>
    {
        public MappingProcessor(Expression<Func<T>> expression)
        {
            
        }
    }
}
