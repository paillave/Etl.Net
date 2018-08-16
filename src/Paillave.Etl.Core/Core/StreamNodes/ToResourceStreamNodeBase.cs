using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.Etl.Helpers;
using SystemIO = System.IO;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.StreamNodes
{
    public abstract class ToResourceStreamArgsBase<TResource> where TResource : IDisposable
    {
        public IStream<TResource> ResourceStream { get; set; }
    }
    public abstract class ToResourceStreamNodeBase<TIn, TResource, TArgs> : AwaitableStreamNodeBase<IStream<TIn>, TIn, TArgs> where TResource : IDisposable
        where TIn : new()
        where TArgs : ToResourceStreamArgsBase<TResource>
    {
        private object _lockObject = new object();
        private TResource _outputResource = default(TResource);
        private Queue<TIn> _onHoldQueue = new Queue<TIn>();

        public ToResourceStreamNodeBase(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, TArgs arguments) : base(input, name, parentNodeNamePath, arguments)
        {
            this.Arguments.ResourceStream.Observable.Subscribe(this.UnstackValues);
        }

        private void UnstackValues(TResource outputResource)
        {
            lock (_lockObject)
            {
                _outputResource = outputResource;
                while (_onHoldQueue.Count > 0)
                    ProcessValueToOutput(_outputResource, _onHoldQueue.Dequeue());
            }
        }

        protected override void ProcessValue(TIn value)
        {
            lock (_lockObject)
            {
                if (_outputResource == null) _onHoldQueue.Enqueue(value);
                else ProcessValueToOutput(_outputResource, value);
            }
        }

        protected abstract void ProcessValueToOutput(TResource outputResource, TIn value);
    }

    //public static partial class StreamEx
    //{
    //    public static IStream<TIn> ToAction<TIn>(this IStream<TIn> stream, string name, Action<TIn> action)
    //    {
    //        return new ToActionStreamNode<TIn>(stream, name, null, action).Output;
    //    }
    //    //public static ISortedStream<TIn> Skip<TIn>(this ISortedStream<TIn> stream, string name, int count)
    //    //{
    //    //    return new SkipSortedStreamNode<TIn>(stream, name, null, count).Output;
    //    //}
    //    //public static IKeyedStream<TIn> Skip<TIn>(this IKeyedStream<TIn> stream, string name, int count)
    //    //{
    //    //    return new SkipKeyedStreamNode<TIn>(stream, name, null, count).Output;
    //    //}
    //}
}
