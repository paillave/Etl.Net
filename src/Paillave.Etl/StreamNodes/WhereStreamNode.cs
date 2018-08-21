using Paillave.Etl.Core;
using Paillave.Etl.Core.NodeOutputs;
using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class WhereArgs<TIn>
    {
        public Func<TIn, bool> Predicate { get; set; }
        public bool RedirectErrorsInsteadOfFail { get; set; } = false;
    }
    public class WhereStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, WhereArgs<TIn>>, IStreamNodeOutput<TIn>, IStreamNodeError<ErrorRow<TIn>>
    {
        public WhereStreamNode(IStream<TIn> input, string name, WhereArgs<TIn> arguments)
            : base(input, name, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = input.Observable.Map(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Filter(i => i.Output).Map(i => i.Input));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn>(i)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), input.Observable.Filter(arguments.Predicate));
        }

        public IStream<TIn> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }

    public class WhereSortedStreamNode<TIn> : StreamNodeBase<ISortedStream<TIn>, TIn, WhereArgs<TIn>>, ISortedStreamNodeOutput<TIn>, IStreamNodeError<ErrorRow<TIn>>
    {
        public WhereSortedStreamNode(ISortedStream<TIn> input, string name, WhereArgs<TIn> arguments)
            : base(input, name, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = input.Observable.Map(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateSortedStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Filter(i => i.Output).Map(i => i.Input), input.SortCriterias);
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn>(i)));
            }
            else
                this.Output = base.CreateSortedStream(nameof(this.Output), input.Observable.Filter(arguments.Predicate), input.SortCriterias);
        }

        public ISortedStream<TIn> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }

    public class WhereKeyedStreamNode<TIn> : StreamNodeBase<IKeyedStream<TIn>, TIn, WhereArgs<TIn>>, IKeyedStreamNodeOutput<TIn>, IStreamNodeError<ErrorRow<TIn>>
    {
        public WhereKeyedStreamNode(IKeyedStream<TIn> input, string name, WhereArgs<TIn> arguments)
            : base(input, name, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = input.Observable.Map(base.ErrorManagementWrapFunction(arguments.Predicate));
                this.Output = base.CreateKeyedStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Filter(i => i.Output).Map(i => i.Input), input.SortCriterias);
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn>(i)));
            }
            else
                this.Output = base.CreateKeyedStream(nameof(this.Output), input.Observable.Filter(arguments.Predicate), input.SortCriterias);
        }

        public IKeyedStream<TIn> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
}
