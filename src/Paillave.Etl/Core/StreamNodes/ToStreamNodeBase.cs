using Paillave.Etl.Core;
using System;
using System.Linq;
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
    public class ToStreamFromOneContextValueArgsBase<TContext> : ToStreamFromOneResourceContextValueArgsBase<TContext, TContext>
    {
        /// <summary>
        /// FROM ONE CONTEXT VALUE
        /// get resource from first ContextStream
        /// </summary>
        /// <param name="contextStream"></param>
        public ToStreamFromOneContextValueArgsBase(IStream<TContext> contextStream) : base(contextStream, i => i)
        {
        }
    }

    public class ToStreamFromOneResourceContextValueArgsBase<TContext, TResource>
    {
        public int ChunkSize { get; set; } = 1;
        public IStream<TContext> ContextStream { get; } = null;
        public Func<TContext, TResource> GetResourceFromContext { get; } = null;

        /// <summary>
        /// FROM ONE RESOURCE CONTEXT VALUE
        /// get resource from first ContextStream using GetResourceFromContext
        /// </summary>
        /// <param name="contextStream"></param>
        /// <param name="getResourceFromContext"></param>
        public ToStreamFromOneResourceContextValueArgsBase(IStream<TContext> contextStream, Func<TContext, TResource> getResourceFromContext)
        {
            ContextStream = contextStream;
            GetResourceFromContext = getResourceFromContext;
        }
    }











    public class ToStreamFromSeveralContextValuesArgsBase<TIn, TContext, TResource, TResourceKey>
    {
        public int ChunkSize { get; set; } = 1;
        public IStream<TContext> ContextStream { get; } = null;
        public Func<TContext, TResourceKey> GetResourceKeyFromContext { get; } = null;
        public Func<TContext, TResource> GetResourceFromContext { get; } = null;
        public Func<TIn, TResourceKey> GetResourceKeyFromInput { get; } = null;

        /// <summary>
        /// FROM SEVERAL CONTEXT VALUES
        /// create a key/resource dictionary from ContextStream using GetResourceKeyFromContext and GetResourceFromContext
        /// get a resource for an input using GetResourceKeyFromInput and the dictionary
        /// </summary>
        /// <param name="contextStream"></param>
        /// <param name="getResourceKeyFromContext"></param>
        /// <param name="getResourceFromContext"></param>
        /// <param name="getResourceKeyFromInput"></param>
        public ToStreamFromSeveralContextValuesArgsBase(IStream<TContext> contextStream, Func<TContext, TResourceKey> getResourceKeyFromContext, Func<TContext, TResource> getResourceFromContext, Func<TIn, TResourceKey> getResourceKeyFromInput)
        {
            ContextStream = contextStream;
            GetResourceKeyFromContext = getResourceKeyFromContext;
            GetResourceFromContext = getResourceFromContext;
            GetResourceKeyFromInput = getResourceKeyFromInput;
        }
    }

    public class ToStreamFromInputValueArgsBase<TIn, TContext, TResource, TResourceKey>
    {
        public int ChunkSize { get; set; } = 1;
        public IStream<TContext> ContextStream { get; } = null;
        public Func<TContext, TIn, TResource> GetResourceFromInput { get; } = null;
        public Func<TIn, TResourceKey> GetResourceKeyFromInput { get; } = null;

        /// <summary>
        /// FROM INPUT VALUE
        /// get key resource for an input using GetResourceKeyFromInput
        /// try get resource from key/resource dictionary 
        /// if nothing found, add entry in dictionary using GetResourceFromInput and the first value of ContextStream
        /// </summary>
        /// <param name="contextStream"></param>
        /// <param name="getResourceFromInput"></param>
        /// <param name="getResourceKeyFromInput"></param>
        public ToStreamFromInputValueArgsBase(IStream<TContext> contextStream, Func<TContext, TIn, TResource> getResourceFromInput, Func<TIn, TResourceKey> getResourceKeyFromInput)
        {
            ContextStream = contextStream;
            GetResourceFromInput = getResourceFromInput;
            GetResourceKeyFromInput = getResourceKeyFromInput;
        }
    }






    //public class ToStreamArgsBase<TResource>
    //{
    //    public int ChunkSize { get; set; } = 1000;
    //    public IStream<TResource> ResourceStream { get; set; }
    //}
    public abstract class ToStreamFromOneResourceContextValueNodeBase<TIn, TContext, TResource, TArgs> : AwaitableStreamNodeBase<IStream<TIn>, TIn, TArgs>
        where TArgs : ToStreamFromOneResourceContextValueArgsBase<TContext, TResource>
    {
        public ToStreamFromOneResourceContextValueNodeBase(IStream<TIn> input, string name, TArgs arguments) : base(input, name, arguments)
        {
        }

        protected override IPushObservable<TIn> ProcessObservable(IPushObservable<TIn> observable)
        {
            var firstResourceS = this.Arguments.ContextStream.Observable.First().Do(i => PreProcess(this.Arguments.GetResourceFromContext(i))).DelayTillEndOfStream();
            if (this.Arguments.ChunkSize == 1)
                return observable
                    .CombineWithLatest(firstResourceS, (i, r) => { ProcessValueToOutput(this.Arguments.GetResourceFromContext( r), i); return i; }, true);
            else
                return observable
                    .Chunk(this.Arguments.ChunkSize)
                    .CombineWithLatest(firstResourceS, (i, r) => { ProcessChunkToOutput(this.Arguments.GetResourceFromContext(r), i); return i; }, true)
                    .FlatMap(i => PushObservable.FromEnumerable(i));
        }

        protected virtual void PreProcess(TResource outputResource) { }

        protected virtual void PreProcessChunk(TResource outputResource, IEnumerable<TIn> values) { }

        protected virtual void PostProcessChunk(TResource outputResource, IEnumerable<TIn> values) { }

        protected virtual void ProcessChunkToOutput(TResource outputResource, IEnumerable<TIn> values)
        {
            PreProcessChunk(outputResource, values);
            foreach (var value in values)
                ProcessValueToOutput(outputResource, value);
            PostProcessChunk(outputResource, values);
        }
        protected virtual void ProcessValueToOutput(TResource outputResource, TIn value) { }
    }
}
