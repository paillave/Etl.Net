namespace Paillave.Etl.Core
{
    public class ValueProviderCorrelationWrapper<TInnerIn, TInnerOut>(IValuesProvider<TInnerIn, TInnerOut> innerValueProvider) 
        : ValueProviderWrapper<Correlated<TInnerIn>, TInnerIn, TInnerOut, Correlated<TInnerOut>>(innerValueProvider, i => i.Row, (i, j) => new Correlated<TInnerOut> { Row = i, CorrelationKeys = j.CorrelationKeys })
    {
    }
}
