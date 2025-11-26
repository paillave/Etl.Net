using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

internal static class ServiceProviderExtensions
{
    public static DbContext GetDbContext(this IServiceProvider services, string? key = null)
        => (key != null
            ? services.GetKeyedService<DbContext>(key)
            : services.GetService<DbContext>())
            ?? throw new InvalidOperationException($"No DbContext could be resolved for type '{typeof(DbContext).FullName}'. Please check your dependency injection configuration.");
}