using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public static partial class AggregateEx
    {
        /// <summary>
        /// Aggregate every element of a stream into a list of aggregations computed for each group by multiple keys
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="emptyAggregation">Initial value of the aggregation per key</param>
        /// <param name="getKeys">Method to get the list of key of an element of the stream</param>
        /// <param name="aggregate">Aggregator that will receive the current value of the aggregation for the key value of the current element and that must return the new aggregation value</param>
        /// <typeparam name="TIn">Main stream type</typeparam>
        /// <typeparam name="TAggr">Aggregation type</typeparam>
        /// <typeparam name="TKeys">Key type</typeparam>
        /// <returns>Output type</returns>
        public static IStream<AggregationResult<TIn, TKeys, TAggr>> AggregateMultiKey<TIn, TAggr, TKeys>(this IStream<TIn> stream, string name, Func<TIn, TKeys> getKeys, Func<TIn, TAggr> emptyAggregation, Func<TAggr, TIn, TAggr> aggregate) =>
            new AggregateMultiKeyStreamNode<TIn, TAggr, TKeys>(name, new AggregateMultiKeyArgs<TIn, TAggr, TKeys>
            {
                InputStream = stream,
                Aggregate = aggregate,
                GetKeys = getKeys,
                CreateEmptyAggregation = emptyAggregation,
            }).Output;
        /// <summary>
        /// Aggregate every element of a stream into a list of aggregations computed for each group by the key
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="emptyAggregation">Initial value of the aggregation per key</param>
        /// <param name="getKey">Method to get the key of an element of the stream</param>
        /// <param name="aggregate">Aggregator that will receive the current value of the aggregation for the key value of the current element and that must return the new aggregation value</param>
        /// <typeparam name="TIn">Main stream type</typeparam>
        /// <typeparam name="TAggr">Aggregation type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <returns>Output type</returns>
        public static IStream<AggregationResult<TIn, TKey, TAggr>> Aggregate<TIn, TAggr, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, Func<TIn, TAggr> emptyAggregation, Func<TAggr, TIn, TAggr> aggregate) =>
            new AggregateStreamNode<TIn, TAggr, TKey>(name, new AggregateArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                Aggregate = aggregate,
                GetKey = getKey,
                CreateEmptyAggregation = emptyAggregation,
            }).Output;
        /// <summary>
        /// Aggregate every element of a sorted stream into a list of aggregations computed for each group by the sorting key
        /// </summary>
        /// <param name="stream">Sorted input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="emptyAggregation">Initial value of the aggregation per key</param>
        /// <param name="aggregate">Aggregator that will receive the current value of the aggregation for the key value of the current element and that must return the new aggregation value</param>
        /// <typeparam name="TIn">Main stream type</typeparam>
        /// <typeparam name="TAggr">Aggregation type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <returns>Output type</returns>
        public static ISortedStream<AggregationResult<TIn, TKey, TAggr>, TKey> Aggregate<TIn, TAggr, TKey>(this ISortedStream<TIn, TKey> stream, string name, Func<TIn, TAggr> emptyAggregation, Func<TAggr, TIn, TAggr> aggregate) =>
            new AggregateSortedStreamNode<TIn, TAggr, TKey>(name, new AggregateSortedArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                Aggregate = aggregate,
                CreateEmptyAggregation = emptyAggregation
            }).Output;

        /// <summary>
        /// Aggregate every element of a stream into a list of aggregations computed for each group by the key
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="emptyAggregation">Initial value of the aggregation per key</param>
        /// <param name="getKey">Method to get the key of an element of the stream</param>
        /// <param name="aggregate">Aggregator that will receive the current value of the aggregation for the key value of the current element and that must return the new aggregation value</param>
        /// <typeparam name="TIn">Main stream type</typeparam>
        /// <typeparam name="TAggr">Aggregation type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <returns>Output type</returns>
        public static IStream<Correlated<AggregationResult<TIn, TKey, TAggr>>> Aggregate<TIn, TAggr, TKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> getKey, Func<TIn, TAggr> emptyAggregation, Func<TAggr, TIn, TAggr> aggregate) =>
            new AggregateCorrelatedStreamNode<TIn, TAggr, TKey>(name, new AggregateCorrelatedArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                Aggregate = aggregate,
                GetKey = getKey,
                CreateEmptyAggregation = emptyAggregation,
            }).Output;
        /// <summary>
        /// Aggregate every element of a sorted stream into a list of aggregations computed for each group by the sorting key
        /// </summary>
        /// <param name="stream">Sorted input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="emptyAggregation">Initial value of the aggregation per key</param>
        /// <param name="aggregate">Aggregator that will receive the current value of the aggregation for the key value of the current element and that must return the new aggregation value</param>
        /// <typeparam name="TIn">Main stream type</typeparam>
        /// <typeparam name="TAggr">Aggregation type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <returns>Output type</returns>
        public static ISortedStream<Correlated<AggregationResult<TIn, TKey, TAggr>>, TKey> Aggregate<TIn, TAggr, TKey>(this ISortedStream<Correlated<TIn>, TKey> stream, string name, Func<TIn, TAggr> emptyAggregation, Func<TAggr, TIn, TAggr> aggregate) =>
            new AggregateCorrelatedSortedStreamNode<TIn, TAggr, TKey>(name, new AggregateCorrelatedSortedArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                Aggregate = aggregate,
                CreateEmptyAggregation = emptyAggregation
            }).Output;

        public static IStream<AggregationResult<TIn, TKey, List<TIn>>> GroupBy<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey) =>
            new AggregateStreamNode<TIn, List<TIn>, TKey>(name, new AggregateArgs<TIn, List<TIn>, TKey>
            {
                InputStream = stream,
                GetKey = getKey,
                Aggregate = (a, v) => { a.Add(v); return a; },
                CreateEmptyAggregation = _ => new List<TIn>(),
            }).Output;
        public static ISortedStream<AggregationResult<TIn, TKey, List<TIn>>, TKey> GroupBy<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name) =>
            new AggregateSortedStreamNode<TIn, List<TIn>, TKey>(name, new AggregateSortedArgs<TIn, List<TIn>, TKey>
            {
                InputStream = stream,
                Aggregate = (a, v) => { a.Add(v); return a; },
                CreateEmptyAggregation = _ => new List<TIn>(),
            }).Output;

        public static IStream<Correlated<AggregationResult<TIn, TKey, List<TIn>>>> GroupBy<TIn, TKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> getKey) =>
            new AggregateCorrelatedStreamNode<TIn, List<TIn>, TKey>(name, new AggregateCorrelatedArgs<TIn, List<TIn>, TKey>
            {
                InputStream = stream,
                GetKey = getKey,
                Aggregate = (a, v) => { a.Add(v); return a; },
                CreateEmptyAggregation = _ => new List<TIn>(),
            }).Output;

        public static ISortedStream<Correlated<AggregationResult<TIn, TKey, List<TIn>>>, TKey> GroupBy<TIn, TKey>(this ISortedStream<Correlated<TIn>, TKey> stream, string name) =>
            new AggregateCorrelatedSortedStreamNode<TIn, List<TIn>, TKey>(name, new AggregateCorrelatedSortedArgs<TIn, List<TIn>, TKey>
            {
                InputStream = stream,
                Aggregate = (a, v) => { a.Add(v); return a; },
                CreateEmptyAggregation = _ => new List<TIn>(),
            }).Output;
    }
}
