using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace Paillave.Etl.Core.System
{
    public abstract class StreamNodeBase
    {
        public StreamNodeBase(ExecutionContextBase executionContext, string name, IEnumerable<string> parentNodeNamePath = null)
        {
            this.Context = executionContext;
            this.NodeNamePath = (parentNodeNamePath ?? new string[] { }).Concat(new[] { name }).ToArray();
        }
        public IEnumerable<string> NodeNamePath { get; private set; }
        protected ExecutionContextBase Context { get; private set; }
        protected Stream<T> CreateStream<T>(string streamName, IObservable<T> observable)
        {
            return new Stream<T>(this.Context, this.NodeNamePath, streamName, observable);
        }
    }
}
