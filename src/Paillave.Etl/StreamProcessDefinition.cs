using System;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl
{
    public class StreamProcessDefinition<TConfig> : IStreamProcessDefinition<TConfig>
    {
        private Action<ISingleStream<TConfig>> _defineJob;
        public StreamProcessDefinition(string name, Action<ISingleStream<TConfig>> defineJob)
        {
            this.Name = name;
            _defineJob = defineJob;
        }
        public StreamProcessDefinition(Action<ISingleStream<TConfig>> defineJob) : this(null, defineJob) { }
        public string Name { get; }
        public void DefineProcess(ISingleStream<TConfig> rootStream) => _defineJob(rootStream);
    }
    public static class StreamProcessDefinition
    {
        public static StreamProcessDefinition<TConfig> Create<TConfig>(string name, Action<ISingleStream<TConfig>> defineJob) => new StreamProcessDefinition<TConfig>(name, defineJob);
        public static StreamProcessDefinition<TConfig> Create<TConfig>(Action<ISingleStream<TConfig>> defineJob) => new StreamProcessDefinition<TConfig>(defineJob);
    }
}
