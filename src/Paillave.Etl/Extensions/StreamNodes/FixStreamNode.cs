﻿using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core
{
    public enum FixApplyCondition
    {
        IfNotNull,
        IfNull,
        Always
    }
    public class FixProperty<T>
    {
        public PropertyInfo PropertyInfo { get; set; }
        public Func<T, object> GetNewValue { get; set; }
        public FixApplyCondition Condition { get; set; }
        public bool MustFix(object val)
        {
            switch (this.Condition)
            {
                case FixApplyCondition.IfNull:
                    return Nullable.Equals(val, null) || (val is string vals && string.IsNullOrWhiteSpace(vals));
                case FixApplyCondition.IfNotNull:
                    return !(Nullable.Equals(val, null) || (val is string val2s && string.IsNullOrWhiteSpace(val2s)));
                default:
                    return true;
            }
        }
    }
    public class Fixer<T>
    {
        private Dictionary<string, FixProperty<T>> _fixes;
        public Fixer() => _fixes = new Dictionary<string, FixProperty<T>>();
        internal Fixer(Dictionary<string, FixProperty<T>> fixes) => _fixes = fixes;
        internal T Fix(T obj)
        {
            var valuesDico = typeof(T).GetProperties().Select(prop =>
                {
                    var val = prop.GetValue(obj);
                    if (_fixes.TryGetValue(prop.Name, out var fix) && fix.MustFix(val))
                        val = fix.GetNewValue(obj);
                    return new { PropName = prop.Name, Value = val };
                }).ToDictionary(i => i.PropName, i => i.Value);
            return ObjectBuilder<T>.CreateInstance(valuesDico);

            // _fixes.ForEach(i => i.Fix(obj));
        }
        public PropertyFixer<T, U> FixProperty<U>(Expression<Func<T, U>> propertyToFix) =>
            new PropertyFixer<T, U>(_fixes, propertyToFix);
    }
    public class PropertyFixer<T, U>(Dictionary<string, FixProperty<T>> fixes, Expression<Func<T, U>> propertyToFix)
    {
        private Dictionary<string, FixProperty<T>> _fixes = fixes;
        private Expression<Func<T, U>> _propertyToFix = propertyToFix;

        private FixProperty<T> GetFix(Func<T, U> getNewValue, FixApplyCondition condition) =>
            new FixProperty<T>
            {
                Condition = condition,
                GetNewValue = i => getNewValue(i),
                PropertyInfo = this._propertyToFix.GetPropertyInfo()
            };
        public Fixer<T> AlwaysWith(Func<T, U> getNewValue)
        {
            var fix = this.GetFix(getNewValue, FixApplyCondition.Always);
            _fixes[fix.PropertyInfo.Name] = fix;
            return new Fixer<T>(_fixes);
        }
        public Fixer<T> IfNullWith(Func<T, U> getNewValue)
        {
            var fix = this.GetFix(getNewValue, FixApplyCondition.IfNull);
            _fixes[fix.PropertyInfo.Name] = fix;
            return new Fixer<T>(_fixes);
        }
        public Fixer<T> IfNotNullWith(Func<T, U> getNewValue)
        {
            var fix = this.GetFix(getNewValue, FixApplyCondition.IfNotNull);
            _fixes[fix.PropertyInfo.Name] = fix;
            return new Fixer<T>(_fixes);
        }
    }

    #region Simple fix null
    public class FixArgs<TIn>
    {
        public IStream<TIn> Stream { get; set; }
        public Fixer<TIn> Fixer { get; set; }
    }
    public class FixStreamNode<TIn>(string name, FixArgs<TIn> args) : StreamNodeBase<TIn, IStream<TIn>, FixArgs<TIn>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TIn> CreateOutputStream(FixArgs<TIn> args)
        {
            IPushObservable<TIn> obs = args.Stream.Observable.Map(args.Fixer.Fix);
            return base.CreateUnsortedStream(obs);
        }
    }
    public class FixCorrelatedArgs<TIn>
    {
        public IStream<Correlated<TIn>> Stream { get; set; }
        public Fixer<TIn> Fixer { get; set; }
    }
    public class FixCorrelatedStreamNode<TIn>(string name, FixCorrelatedArgs<TIn> args) : StreamNodeBase<Correlated<TIn>, IStream<Correlated<TIn>>, FixCorrelatedArgs<TIn>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<Correlated<TIn>> CreateOutputStream(FixCorrelatedArgs<TIn> args)
        {
            IPushObservable<Correlated<TIn>> obs = args.Stream.Observable.Map(i => new Correlated<TIn> { Row = args.Fixer.Fix(i.Row), CorrelationKeys = i.CorrelationKeys });
            return base.CreateUnsortedStream(obs);
        }
    }
    #endregion

    #region Simple Single fix null
    public class FixSingleArgs<TIn>
    {
        public ISingleStream<TIn> Stream { get; set; }
        public Fixer<TIn> Fixer { get; set; }
    }
    public class FixSingleStreamNode<TIn>(string name, FixSingleArgs<TIn> args) : StreamNodeBase<TIn, ISingleStream<TIn>, FixSingleArgs<TIn>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<TIn> CreateOutputStream(FixSingleArgs<TIn> args)
        {
            IPushObservable<TIn> obs = args.Stream.Observable.Map(args.Fixer.Fix);
            return base.CreateSingleStream(obs);
        }
    }
    public class FixCorrelatedSingleArgs<TIn>
    {
        public ISingleStream<Correlated<TIn>> Stream { get; set; }
        public Fixer<TIn> Fixer { get; set; }
    }
    public class FixCorrelatedSingleStreamNode<TIn>(string name, FixCorrelatedSingleArgs<TIn> args) : StreamNodeBase<Correlated<TIn>, ISingleStream<Correlated<TIn>>, FixCorrelatedSingleArgs<TIn>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<Correlated<TIn>> CreateOutputStream(FixCorrelatedSingleArgs<TIn> args)
        {
            IPushObservable<Correlated<TIn>> obs = args.Stream.Observable.Map(i => new Correlated<TIn> { Row = args.Fixer.Fix(i.Row), CorrelationKeys = i.CorrelationKeys });
            return base.CreateSingleStream(obs);
        }
    }
    #endregion
}
