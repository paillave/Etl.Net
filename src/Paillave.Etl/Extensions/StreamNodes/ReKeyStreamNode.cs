using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.Core
{
    public class ReKeyArgs<TIn, TOut, TMultiKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TIn, TMultiKey, TOut> ResultSelector { get; set; }
        public Func<TIn, TMultiKey> GetKeys { get; set; }
    }
    public class ReKeyStreamNode<TIn, TOut, TMultiKey> : StreamNodeBase<TOut, IStream<TOut>, ReKeyArgs<TIn, TOut, TMultiKey>>
    {
        public ReKeyStreamNode(string name, ReKeyArgs<TIn, TOut, TMultiKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<TOut> CreateOutputStream(ReKeyArgs<TIn, TOut, TMultiKey> args)
        {
            var keyProcessor = GroupProcessor.Create(args.GetKeys);
            var observableOut = args.InputStream.Observable.Do(keyProcessor.ProcessRow).Last().MultiMap<TIn, TOut>((i, pushValue) =>
            {
                var groups = keyProcessor.GetGroups();
                foreach (var elt in groups.SelectMany(g => g.Value.Select(v => new { Row = v, g.Key })))
                {
                    pushValue(args.ResultSelector(elt.Row, elt.Key));
                }
            });
            return CreateUnsortedStream(observableOut);
        }
    }









    public class ReKey2Args<TIn, TRow, TOut>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TIn, TRow> RowSelector { get; set; }
        public Func<TIn, TRow, TOut> ResultSelector { get; set; }
        public Expression<Func<TRow, object>> GetKeys { get; set; }
    }
    public class ReKey2StreamNode<TIn, TRow, TOut> : StreamNodeBase<TOut, IStream<TOut>, ReKey2Args<TIn, TRow, TOut>>
    {
        public ReKey2StreamNode(string name, ReKey2Args<TIn, TRow, TOut> args) : base(name, args)
        {
            if (args.GetKeys.GetPropertyInfos().Count != 2)
            {
                throw new ArgumentException($"{name}: The rekey accepts only 2 keys");
            }
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<TOut> CreateOutputStream(ReKey2Args<TIn, TRow, TOut> args)
        {
            var observableOut = args.InputStream.Observable.ToList().MultiMap<List<TIn>, TOut>(ProcessList);
            return CreateUnsortedStream(observableOut);
        }
        private void ProcessList(List<TIn> inputs, Action<TOut> pushValue)
        {
            var pis = Args.GetKeys.GetPropertyInfos();
            var pi1 = pis[0];
            var pi2 = pis[1];
            var items = inputs.Select(i =>
            {
                var row = Args.RowSelector(i);
                return new
                {
                    Input = i,
                    Row = row,
                    Key1 = pi1.GetValue(row),
                    Key2 = pi2.GetValue(row)
                };
            }).ToList();
            var key1Dico = items.Where(i => i.Key1 != null && i.Key2 != null).GroupBy(i => i.Key1).ToDictionary(i => i.Key, i => i.First());
            var key2Dico = items.Where(i => i.Key2 != null && i.Key2 != null).GroupBy(i => i.Key2).ToDictionary(i => i.Key, i => i.First());
            foreach (var item in items)
            {
                var propertyInfos = item.Row.GetType().GetProperties();
                var values = propertyInfos.ToDictionary(i => i.Name, i => i.GetValue(item.Row));
                if (item.Key1 != null && key1Dico.TryGetValue(item.Key1, out var key1Row))
                    values[pi2.Name] = key1Row.Key2;
                else if (item.Key1 == null && item.Key2 != null && key2Dico.TryGetValue(item.Key2, out var key2Row))
                    values[pi1.Name] = key2Row.Key1;
                pushValue(Args.ResultSelector(item.Input, (TRow)ObjectBuilder.CreateInstance(item.Row.GetType(), values)));
            }
        }
    }
}
