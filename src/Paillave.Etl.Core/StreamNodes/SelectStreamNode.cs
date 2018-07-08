using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Paillave.Etl.Core.StreamNodes
{
    public class SelectArgs<TIn, TOut>
    {
        public Func<TIn, TOut> Mapper { get; set; } = null;
        public Func<TIn, int, TOut> IndexMapper { get; set; } = null;
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class SelectStreamNode<TIn, TOut> : StreamNodeBase<IStream<TIn>, TIn, SelectArgs<TIn, TOut>>, IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn>>
    {
        public SelectStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, SelectArgs<TIn, TOut> arguments) : base(input, name, parentNodeNamePath, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = arguments.Mapper == null ?
                    input.Observable.Select(base.ErrorManagementWrapFunction(arguments.IndexMapper))
                    : input.Observable.Select(base.ErrorManagementWrapFunction(arguments.Mapper));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn>(i.Input, i.Exception)));
            }
            else
                this.Output = arguments.Mapper == null ?
                    base.CreateStream(nameof(this.Output), input.Observable.Select(arguments.IndexMapper))
                    : base.CreateStream(nameof(this.Output), input.Observable.Select(arguments.Mapper));

            //Observable.Create()
        }

        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
    public static partial class StreamEx
    {
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper)
        {
            return new SelectStreamNode<TIn, TOut>(stream, name, null, new SelectArgs<TIn, TOut>
            {
                Mapper = mapper,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> mapper)
        {
            return new SelectStreamNode<TIn, TOut>(stream, name, null, new SelectArgs<TIn, TOut>
            {
                IndexMapper = mapper,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static NodeOutputError<TOut, TIn> SelectKeepErrors<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper)
        {
            var ret = new SelectStreamNode<TIn, TOut>(stream, name, null, new SelectArgs<TIn, TOut>
            {
                Mapper = mapper,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<TOut, TIn>(ret.Output, ret.Error);
        }
        public static NodeOutputError<TOut, TIn> SelectKeepErrors<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> mapper)
        {
            var ret = new SelectStreamNode<TIn, TOut>(stream, name, null, new SelectArgs<TIn, TOut>
            {
                IndexMapper = mapper,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<TOut, TIn>(ret.Output, ret.Error);
        }
    }
}
