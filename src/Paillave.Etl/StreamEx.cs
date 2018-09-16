using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SystemIO = System.IO;

namespace Paillave.Etl
{
    public static class StreamEx
    {
        #region Distinct
        public static IStream<TIn> Distinct<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey)
        {
            return new DistinctStreamNode<TIn, TKey>(name, new DistinctArgs<TIn, TKey>
            {
                GetKey = getKey,
                InputStream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Distinct<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name)
        {
            return new DistinctSortedStreamNode<TIn, TKey>(name, new DistinctSortedArgs<TIn, TKey>
            {
                InputStream = stream
            }).Output;
        }
        #endregion

        #region Pivot
        public static IStream<AggregationResult<TIn, TKey, TAggr>> Pivot<TIn, TAggr, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, Expression<Func<TIn, TAggr>> aggregationDescriptor)
        {
            return new PivotStreamNode<TIn, TAggr, TKey>(name, new PivotArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                AggregationDescriptor = aggregationDescriptor,
                GetKey = getKey,
            }).Output;
        }
        public static ISortedStream<AggregationResult<TIn, TKey, TAggr>, TKey> Pivot<TIn, TAggr, TKey>(this ISortedStream<TIn, TKey> stream, string name, Expression<Func<TIn, TAggr>> aggregationDescriptor)
        {
            return new PivotSortedStreamNode<TIn, TAggr, TKey>(name, new PivotSortedArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                AggregationDescriptor = aggregationDescriptor
            }).Output;
        }
        #endregion

        #region Unpivot
        public static IStream<TOut> Unpivot<TIn, TUnpivoted, TOut>(this IStream<TIn> stream, string name, IEnumerable<Func<TIn, TUnpivoted>> fieldsToUnpivot, Func<TIn, TUnpivoted, TOut> resultSelector)
        {
            return new UnpivotStreamNode<TIn, TUnpivoted, TOut>(name, new UnpivotArgs<TIn, TUnpivoted, TOut>
            {
                InputStream = stream,
                FieldsToUnpivot = fieldsToUnpivot,
                ResultSelector = resultSelector
            }).Output;
        }
        #endregion

        #region Aggregate
        public static IStream<AggregationResult<TIn, TKey, TAggr>> Aggregate<TIn, TAggr, TKey>(this IStream<TIn> stream, string name, Func<TIn, TAggr> emptyAggregation, Func<TIn, TKey> getKey, Func<TAggr, TIn, TAggr> aggregate)
        {
            return new AggregateStreamNode<TIn, TAggr, TKey>(name, new AggregateArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                Aggregate = aggregate,
                GetKey = getKey,
                CreateEmptyAggregation = emptyAggregation,
            }).Output;
        }
        public static ISortedStream<AggregationResult<TIn, TKey, TAggr>, TKey> Aggregate<TIn, TAggr, TKey>(this ISortedStream<TIn, TKey> stream, string name, Func<TIn, TAggr> emptyAggregation, Func<TAggr, TIn, TAggr> aggregate)
        {
            return new AggregateSortedStreamNode<TIn, TAggr, TKey>(name, new AggregateSortedArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                Aggregate = aggregate,
                CreateEmptyAggregation = emptyAggregation
            }).Output;
        }
        #endregion

        #region CrossApply
        public static IStream<TOut> CrossApply<TIn, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TValueIn, TValueOut> valuesProvider, Func<TIn, TValueIn> inputValueSelector, Func<TValueOut, TIn, TOut> outputValueSelector)
        {
            return new CrossApplyStreamNode<TIn, TValueIn, TValueOut, TOut>(name, new CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>
            {
                Stream = stream,
                GetValueIn = inputValueSelector,
                GetValueOut = outputValueSelector,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TIn, TOut> valuesProvider)
        {
            return new CrossApplyStreamNode<TIn, TIn, TOut, TOut>(name, new CrossApplyArgs<TIn, TIn, TOut, TOut>
            {
                Stream = stream,
                GetValueIn = i => i,
                GetValueOut = (i, j) => i,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TRes, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, IValuesProvider<TValueIn, TRes, TValueOut> valuesProvider, Func<TIn, TRes, TValueIn> inputValueSelector, Func<TValueOut, TIn, TRes, TOut> outputValueSelector)
        {
            return new CrossApplyStreamNode<TIn, TRes, TValueIn, TValueOut, TOut>(name, new CrossApplyArgs<TIn, TRes, TValueIn, TValueOut, TOut>
            {
                MainStream = stream,
                GetValueIn = inputValueSelector,
                GetValueOut = outputValueSelector,
                ValuesProvider = valuesProvider,
                StreamToApply = resourceStream
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, IValuesProvider<TIn, TRes, TOut> valuesProvider)
        {
            return new CrossApplyStreamNode<TIn, TRes, TIn, TOut, TOut>(name, new CrossApplyArgs<TIn, TRes, TIn, TOut, TOut>
            {
                MainStream = stream,
                GetValueIn = (i, j) => i,
                GetValueOut = (i, j, k) => i,
                ValuesProvider = valuesProvider,
                StreamToApply = resourceStream
            }).Output;
        }
        #endregion

        #region CrossApplyAction
        public static IStream<TOut> CrossApplyAction<TIn, TOut>(this IStream<TIn> stream, string name, Action<TIn, Action<TOut>> valuesProducer, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new ActionValuesProvider<TIn, TOut>(new ActionValuesProviderArgs<TIn, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = valuesProducer
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyAction<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, Action<TIn, TRes, Action<TOut>> valuesProducer, bool noParallelisation = false)
        {
            return stream.CrossApply(name, resourceStream, new ActionResourceValuesProvider<TIn, TRes, TOut>(new ActionResourceValuesProviderArgs<TIn, TRes, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = valuesProducer
            }), (i, _) => i, (i, _, __) => i);
        }
        #endregion

        #region EnsureKeyed
        public static IKeyedStream<TIn, TKey> EnsureKeyed<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, object sortPositions = null)
        {
            return new EnsureKeyedStreamNode<TIn, TKey>(name, new EnsureKeyedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create(getKey, sortPositions)
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> EnsureKeyed<TIn, TKey>(this IStream<TIn> stream, string name, SortDefinition<TIn, TKey> sortDefinition)
        {
            return new EnsureKeyedStreamNode<TIn, TKey>(name, new EnsureKeyedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = sortDefinition
            }).Output;
        }
        #endregion

        #region EnsureSorted
        public static ISortedStream<TIn, TKey> EnsureSorted<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, object sortPositions = null)
        {
            return new EnsureSortedStreamNode<TIn, TKey>(name, new EnsureSortedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create(getKey, sortPositions)
            }).Output;
        }
        public static ISortedStream<TIn, TKey> EnsureSorted<TIn, TKey>(this IStream<TIn> stream, string name, SortDefinition<TIn, TKey> sortDefinition)
        {
            return new EnsureSortedStreamNode<TIn, TKey>(name, new EnsureSortedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = sortDefinition
            }).Output;
        }
        #endregion

        #region LeftJoin
        public static IStream<TOut> LeftJoin<TInLeft, TInRight, TOut, TKey>(this ISortedStream<TInLeft, TKey> leftStream, string name, IKeyedStream<TInRight, TKey> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            return new JoinStreamNode<TInLeft, TInRight, TOut, TKey>(name, new JoinArgs<TInLeft, TInRight, TOut, TKey>
            {
                LeftInputStream = leftStream,
                RightInputStream = rightStream,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        #endregion

        #region Lookup
        public static IStream<TOut> Lookup<TInLeft, TInRight, TOut, TKey>(this IStream<TInLeft> leftStream, string name, IStream<TInRight> rightStream, Func<TInLeft, TKey> leftKey, Func<TInRight, TKey> rightKey, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            return new LookupStreamNode<TInLeft, TInRight, TOut, TKey>(name, new LookupArgs<TInLeft, TInRight, TOut, TKey>
            {
                LeftInputStream = leftStream,
                RightInputStream = rightStream,
                ResultSelector = resultSelector,
                GetLeftStreamKey = leftKey,
                GetRightStreamKey = rightKey
            }).Output;
        }
        #endregion

        #region Union
        public static IStream<I> Union<I>(this IStream<I> stream, string name, IStream<I> inputStream2)
        {
            return new UnionStreamNode<I>(name, new UnionArgs<I>
            {
                Stream1 = stream,
                Stream2 = inputStream2
            }).Output;
        }
        #endregion

        #region Select
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper, bool excludeNull = false)
        {
            return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
            {
                Stream = stream,
                Selector = mapper,
                ExcludeNull = excludeNull
            }).Output;
        }
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> mapper, bool excludeNull = false)
        {
            return new SelectStreamNode<TIn, TOut>(name, new SelectArgs<TIn, TOut>
            {
                Stream = stream,
                IndexSelector = mapper,
                ExcludeNull = excludeNull
            }).Output;
        }
        #endregion

        #region Skip
        public static ISortedStream<TIn, TKey> Skip<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, int count)
        {
            return new SkipStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new SkipArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Skip<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, int count)
        {
            return new SkipStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new SkipArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        public static IStream<TIn> Skip<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new SkipStreamNode<TIn, IStream<TIn>>(name, new SkipArgs<TIn, IStream<TIn>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        #endregion

        #region Sort
        public static ISortedStream<TIn, TKey> Sort<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, object keyPositions = null)
        {
            return new SortStreamNode<TIn, TKey>(name, new SortArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create(getKey, keyPositions)
            }).Output;
        }
        public static ISortedStream<TIn, TKey> Sort<TIn, TKey>(this IStream<TIn> stream, string name, SortDefinition<TIn, TKey> sortDefinition)
        {
            return new SortStreamNode<TIn, TKey>(name, new SortArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = sortDefinition
            }).Output;
        }
        #endregion

        #region ToAction
        public static IStream<TIn> ToAction<TIn>(this IStream<TIn> stream, string name, Action<TIn> processRow)
        {
            return new ToActionStreamNode<TIn, IStream<TIn>>(name, new ToActionArgs<TIn, IStream<TIn>>
            {
                ProcessRow = processRow,
                Stream = stream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToAction<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, Action<TIn> processRow)
        {
            return new ToActionStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ToActionArgs<TIn, ISortedStream<TIn, TKey>>
            {
                ProcessRow = processRow,
                Stream = stream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToAction<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, Action<TIn> processRow)
        {
            return new ToActionStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ToActionArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                ProcessRow = processRow,
                Stream = stream
            }).Output;
        }
        public static IStream<TIn> ToAction<TIn, TResource>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ToActionStreamNode<TIn, IStream<TIn>, TResource>(name, new ToActionArgs<TIn, IStream<TIn>, TResource>
            {
                ProcessRow = processRow,
                Stream = stream,
                ResourceStream = resourceStream,
                PreProcess = preProcess
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToAction<TIn, TResource, TKey>(this ISortedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ToActionStreamNode<TIn, ISortedStream<TIn, TKey>, TResource>(name, new ToActionArgs<TIn, ISortedStream<TIn, TKey>, TResource>
            {
                ProcessRow = processRow,
                Stream = stream,
                ResourceStream = resourceStream,
                PreProcess = preProcess
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToAction<TIn, TResource, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            return new ToActionStreamNode<TIn, IKeyedStream<TIn, TKey>, TResource>(name, new ToActionArgs<TIn, IKeyedStream<TIn, TKey>, TResource>
            {
                ProcessRow = processRow,
                Stream = stream,
                ResourceStream = resourceStream,
                PreProcess = preProcess
            }).Output;
        }
        #endregion

        #region Top
        public static ISortedStream<TIn, TKey> Top<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, int count)
        {
            return new TopStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new TopArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Top<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, int count)
        {
            return new TopStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new TopArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        public static IStream<TIn> Top<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new TopStreamNode<TIn, IStream<TIn>>(name, new TopArgs<TIn, IStream<TIn>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        #endregion

        #region Where
        public static IKeyedStream<TIn, TKey> Where<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new WhereArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Input = stream,
                Predicate = predicate
            }).Output;
        }
        public static ISortedStream<TIn, TKey> Where<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new WhereArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Input = stream,
                Predicate = predicate
            }).Output;
        }
        public static IStream<TIn> Where<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn, IStream<TIn>>(name, new WhereArgs<TIn, IStream<TIn>>
            {
                Input = stream,
                Predicate = predicate
            }).Output;
        }
        #endregion

        #region Apply
        public static IStream<TOut> Select<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> streamToApply, Func<TIn1, TIn2, int, TOut> resultSelector, bool excludeNull = false)
        {
            return new ApplyStreamNode<TIn1, TIn2, TOut>(name, new ApplyArgs<TIn1, TIn2, TOut>
            {
                MainStream = stream,
                StreamToApply = streamToApply,
                IndexSelector = resultSelector,
                ExcludeNull = excludeNull
            }).Output;
        }
        public static IStream<TOut> Select<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> streamToApply, Func<TIn1, TIn2, TOut> resultSelector, bool excludeNull = false)
        {
            return new ApplyStreamNode<TIn1, TIn2, TOut>(name, new ApplyArgs<TIn1, TIn2, TOut>
            {
                MainStream = stream,
                StreamToApply = streamToApply,
                Selector = resultSelector,
                ExcludeNull = excludeNull
            }).Output;
        }
        #endregion

        #region SelectStreamStatistics
        public static Task<StreamStatistics> GetStreamStatisticsAsync(this IStream<TraceEvent> input)
        {
            var errorsStatistics = input
                .Where("keep errors", i => i.Content.Level == System.Diagnostics.TraceLevel.Error)
                .Select("select errors caracteristics", i => new StreamStatisticError { NodeName = i.NodeName, Text = i.ToString() })
                .Observable.ToList();
            var streamStatistics = input
                .Where("keep stream results", i => i.Content is CounterSummaryStreamTraceContent)
                .Select("select statistic", i =>
             {
                 var content = (CounterSummaryStreamTraceContent)i.Content;
                 return new StreamStatisticCounter
                 {
                     Counter = content.Counter,
                     SourceNodeName = i.NodeName
                 };
             }).Observable.ToList();
            return streamStatistics
                .CombineWithLatest(errorsStatistics, (s, e) => new StreamStatistics
                {
                    StreamStatisticErrors = e,
                    StreamStatisticCounters = s
                })
                .ToTaskAsync();
        }
        #endregion

        #region SubProcess
        public static IStream<TOut> SubExecute<TIn, TOut>(this IStream<TIn> stream, string name, Func<IStream<TIn>, IStream<TOut>> subProcess, bool noParallelisation = false)
        {
            return new SubProcessStreamNode<TIn, TOut>(name, new SubProcessArgs<TIn, TOut>
            {
                NoParallelisation = noParallelisation,
                SimpleSubProcess = subProcess,
                Stream = stream
            }).Output;
        }
        #endregion
    }
}
