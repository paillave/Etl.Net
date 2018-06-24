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
            this.ExecutionContext = executionContext;
            this.NodeNamePath = (parentNodeNamePath ?? new string[] { }).Concat(new[] { name }).ToArray();
        }
        public IEnumerable<string> NodeNamePath { get; private set; }
        protected ExecutionContextBase ExecutionContext { get; private set; }
    }
}
