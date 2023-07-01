namespace Paillave.Etl.Core
{
    public class ValueProviderCorrelationWrapper<TInnerIn, TInnerOut> : ValueProviderWrapper<Correlated<TInnerIn>, TInnerIn, TInnerOut, Correlated<TInnerOut>>
    {
        public ValueProviderCorrelationWrapper(IValuesProvider<TInnerIn, TInnerOut> innerValueProvider) :
            base(innerValueProvider, i => i.Row, (i, j) => new Correlated<TInnerOut> { Row = i, CorrelationKeys = j.CorrelationKeys })
        {
        }
    }
}
