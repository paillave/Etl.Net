using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core
{
    public interface IMultiService { }
    public class MultiService<TS1, TS2> : IMultiService
    {
        public TS1 Service1 { get; set; }
        public TS2 Service2 { get; set; }
    }
    public class MultiService<TS1, TS2, TS3> : MultiService<TS1, TS2>, IMultiService
    {
        public TS3 Service3 { get; set; }
    }
    public class MultiService<TS1, TS2, TS3, TS4> : MultiService<TS1, TS2, TS3>, IMultiService
    {
        public TS4 Service4 { get; set; }
    }
    public class MultiService<TS1, TS2, TS3, TS4, TS5> : MultiService<TS1, TS2, TS3, TS4>, IMultiService
    {
        public TS5 Service5 { get; set; }
    }
    public class MultiService<TS1, TS2, TS3, TS4, TS5, TS6> : MultiService<TS1, TS2, TS3, TS4, TS5>, IMultiService
    {
        public TS6 Service6 { get; set; }
    }
    public class Resolver<TIn>
    {
        private IDependencyResolver _dependencyResolver;
        public Resolver(IDependencyResolver dependencyResolver) => _dependencyResolver = dependencyResolver;
        public ResolverSelector<TIn, TService> Resolve<TService>() => new ResolverSelector<TIn, TService>(this, null);
        public ResolverSelector<TIn, MultiService<TService1, TService2>> Resolve<TService1, TService2>() => new ResolverSelector<TIn, MultiService<TService1, TService2>>(this, null);
        public ResolverSelector<TIn, MultiService<TService1, TService2, TService3>> Resolve<TService1, TService2, TService3>() => new ResolverSelector<TIn, MultiService<TService1, TService2, TService3>>(this, null);
        public ResolverSelector<TIn, MultiService<TService1, TService2, TService3, TService4>> Resolve<TService1, TService2, TService3, TService4>() => new ResolverSelector<TIn, MultiService<TService1, TService2, TService3, TService4>>(this, null);
        public ResolverSelector<TIn, MultiService<TService1, TService2, TService3, TService4, TService5>> Resolve<TService1, TService2, TService3, TService4, TService5>() => new ResolverSelector<TIn, MultiService<TService1, TService2, TService3, TService4, TService5>>(this, null);
        public ResolverSelector<TIn, MultiService<TService1, TService2, TService3, TService4, TService5, TService6>> Resolve<TService1, TService2, TService3, TService4, TService5, TService6>() => new ResolverSelector<TIn, MultiService<TService1, TService2, TService3, TService4, TService5, TService6>>(this, null);
        public ResolverSelector<TIn, TService> Resolve<TService>(string serviceKey) => new ResolverSelector<TIn, TService>(this, serviceKey);
        internal TService ResolveService<TService>()
        {
            Type type = typeof(TService);
            if (typeof(IMultiService).IsAssignableFrom(type))
            {
                var ret = (TService)Activator.CreateInstance(type);
                foreach (var property in type.GetProperties())
                    property.SetValue(ret, _dependencyResolver.Resolve(property.PropertyType));
                return ret;
            }
            else
                return _dependencyResolver.Resolve<TService>();
        }
        internal TService ResolveService<TService>(string serviceKey) => _dependencyResolver.Resolve<TService>(serviceKey);
    }
    public class ResolverSelector<TIn, TService>
    {
        private Resolver<TIn> _resolver;
        private string _serviceKey;
        internal ResolverSelector(Resolver<TIn> resolver, string serviceKey) => (_resolver, _serviceKey) = (resolver, serviceKey);
        public Selector<TIn, TService, TOut> Select<TOut>(Func<TIn, TService, TOut> select) => new Selector<TIn, TService, TOut>(this, select);
        internal TService ResolveService() => _serviceKey == null ? _resolver.ResolveService<TService>() : _resolver.ResolveService<TService>(_serviceKey);
    }
    public class Selector<TIn, TService, TOut>
    {
        private ResolverSelector<TIn, TService> _resolverSelector;
        private Func<TIn, TService, TOut> _select;
        public Selector(ResolverSelector<TIn, TService> resolverSelector, Func<TIn, TService, TOut> select) => (_resolverSelector, _select) = (resolverSelector, select);
        internal TOut GetValue(TIn input) => _select(input, _resolverSelector.ResolveService());
    }
    #region Simple select
    public class ResolveAndSelectArgs<TIn, TService, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<Resolver<TIn>, Selector<TIn, TService, TOut>> Selection { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class ResolveAndSelectStreamNode<TIn, TService, TOut> : StreamNodeBase<TOut, IStream<TOut>, ResolveAndSelectArgs<TIn, TService, TOut>>
    {
        public ResolveAndSelectStreamNode(string name, ResolveAndSelectArgs<TIn, TService, TOut> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override IStream<TOut> CreateOutputStream(ResolveAndSelectArgs<TIn, TService, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectForDisposal<TIn, TOut>(i => args.Selection(new Resolver<TIn>(this.ExecutionContext.DependencyResolver)).GetValue(i), args.WithNoDispose));
            return base.CreateUnsortedStream(obs);
        }
    }
    #endregion
    #region Simple Single select
    public class ResolveAndSelectSingleArgs<TIn, TService, TOut>
    {
        public ISingleStream<TIn> Stream { get; set; }
        public Func<Resolver<TIn>, Selector<TIn, TService, TOut>> Selection { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class ResolveAndSelectSingleStreamNode<TIn, TService, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, ResolveAndSelectSingleArgs<TIn, TService, TOut>>
    {
        public ResolveAndSelectSingleStreamNode(string name, ResolveAndSelectSingleArgs<TIn, TService, TOut> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<TOut> CreateOutputStream(ResolveAndSelectSingleArgs<TIn, TService, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectForDisposal<TIn, TOut>(i => args.Selection(new Resolver<TIn>(this.ExecutionContext.DependencyResolver)).GetValue(i), args.WithNoDispose));
            return base.CreateSingleStream(obs);
        }
    }
    #endregion
    #region Simple correlated select
    public class ResolveAndSelectCorrelatedArgs<TIn, TService, TOut>
    {
        public IStream<Correlated<TIn>> Stream { get; set; }
        public Func<Resolver<TIn>, Selector<TIn, TService, TOut>> Selection { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class ResolveAndSelectCorrelatedStreamNode<TIn, TService, TOut> : StreamNodeBase<Correlated<TOut>, IStream<Correlated<TOut>>, ResolveAndSelectCorrelatedArgs<TIn, TService, TOut>>
    {
        public ResolveAndSelectCorrelatedStreamNode(string name, ResolveAndSelectCorrelatedArgs<TIn, TService, TOut> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override IStream<Correlated<TOut>> CreateOutputStream(ResolveAndSelectCorrelatedArgs<TIn, TService, TOut> args)
        {
            IPushObservable<Correlated<TOut>> obs = args.Stream.Observable.Map(WrapSelectCorrelatedForDisposal<TIn, TOut>(i => new Correlated<TOut>
            {
                Row = args.Selection(new Resolver<TIn>(this.ExecutionContext.DependencyResolver)).GetValue(i.Row),
                CorrelationKeys = i.CorrelationKeys
            }, args.WithNoDispose));
            return base.CreateUnsortedStream(obs);
        }
    }
    #endregion
    #region Simple Single correlated select
    public class ResolveAndSelectCorrelatedSingleArgs<TIn, TService, TOut>
    {
        public ISingleStream<Correlated<TIn>> Stream { get; set; }
        public Func<Resolver<TIn>, Selector<TIn, TService, TOut>> Selection { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class ResolveAndSelectCorrelatedSingleStreamNode<TIn, TService, TOut> : StreamNodeBase<Correlated<TOut>, ISingleStream<Correlated<TOut>>, ResolveAndSelectCorrelatedSingleArgs<TIn, TService, TOut>>
    {
        public ResolveAndSelectCorrelatedSingleStreamNode(string name, ResolveAndSelectCorrelatedSingleArgs<TIn, TService, TOut> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<Correlated<TOut>> CreateOutputStream(ResolveAndSelectCorrelatedSingleArgs<TIn, TService, TOut> args)
        {
            IPushObservable<Correlated<TOut>> obs = args.Stream.Observable.Map(WrapSelectCorrelatedForDisposal<TIn, TOut>(i => new Correlated<TOut>
            {
                Row = args.Selection(new Resolver<TIn>(this.ExecutionContext.DependencyResolver)).GetValue(i.Row),
                CorrelationKeys = i.CorrelationKeys
            }
            , args.WithNoDispose));
            return base.CreateSingleStream(obs);
        }
    }
    #endregion
}
