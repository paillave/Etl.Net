using Paillave.Etl.Core;
using System;
using System.Linq;

namespace Paillave.Etl.Core;

public class CompositeServiceProvider(params IServiceProvider?[] serviceProviders) : IServiceProvider
{
    public object GetService(Type serviceType)
    {
        foreach (var serviceProvider in serviceProviders.Where(sp => sp != null))
        {
            var service = serviceProvider!.GetService(serviceType);
            if (service != null)
                return service;
        }
        return null;
    }
}
