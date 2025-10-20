using System;

namespace Paillave.Etl.Core
{
    public abstract class ProviderProcessorAdapterBase<TConnectionParameters, TProviderParameters, TProcessorParameters> : IProviderProcessorAdapter
    {
        public Type ConnectionParametersType => typeof(TConnectionParameters);
        public virtual string Name => this.GetType().Name.Replace("ProviderProcessorAdapter", "");
        public virtual string Description => this.Name;
        public Type ProviderParametersType => typeof(TProviderParameters) == typeof(object) ? null : typeof(TProviderParameters);
        public Type ProcessorParametersType => typeof(TProcessorParameters) == typeof(object) ? null : typeof(TProcessorParameters);

        public IFileValueProvider CreateProvider(string code, string name, string connectionName, object connectionParameters, object inputParameters)
            => CreateProvider(code, name, connectionName, (TConnectionParameters)connectionParameters, (TProviderParameters)inputParameters);

        public IFileValueProcessor CreateProcessor(string code, string name, string connectionName, object connectionParameters, object outputParameters)
            => CreateProcessor(code, name, connectionName, (TConnectionParameters)connectionParameters, (TProcessorParameters)outputParameters);

        protected abstract IFileValueProvider CreateProvider(string code, string name, string connectionName, TConnectionParameters connectionParameters, TProviderParameters inputParameters);

        protected abstract IFileValueProcessor CreateProcessor(string code, string name, string connectionName, TConnectionParameters connectionParameters, TProcessorParameters outputParameters);
    }
    public class SensitiveAttribute : Attribute
    {
    }
}