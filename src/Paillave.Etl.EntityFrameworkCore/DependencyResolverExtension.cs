using System;
using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;

namespace Paillave.Etl.EntityFrameworkCore;

public static class DependencyResolverExtension
{
    public static DisposeWrapper<DbContext>? ResolveDbContextWrapper<TDbContext>(this IDependencyResolver dependencyResolver, string? keyedConnection = null) where TDbContext : DbContext
        => dependencyResolver.ResolveDbContextWrapper(typeof(TDbContext), keyedConnection);
    public static DisposeWrapper<DbContext>? ResolveDbContextWrapper(this IDependencyResolver dependencyResolver, Type? dbContextType, string? keyedConnection = null)
    {
        dbContextType ??= typeof(DbContext);
        var dbContextFactoryType = typeof(IDbContextFactory<>).MakeGenericType(dbContextType);
        if (keyedConnection == null)
        {
            if (dependencyResolver.TryResolve(dbContextFactoryType, out var dbContextFactory))
            {
                var contextFactory = (DbContext)dependencyResolver.Resolve(dbContextType);
                var dbContext = dbContextFactoryType.GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext))?.Invoke(contextFactory, null)!;
                return new DisposeWrapper<DbContext>((DbContext)dbContext, true);
            }
            else if (dependencyResolver.TryResolve(dbContextType, out var dbContext))
            {
                return new DisposeWrapper<DbContext>((DbContext)dbContext, false);
            }
            return null;
        }
        else
        {
            if (dependencyResolver.TryResolve(dbContextFactoryType, keyedConnection, out var dbContextFactory))
            {
                var contextFactory = (DbContext)dependencyResolver.Resolve(dbContextType);
                var dbContext = dbContextFactoryType.GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext))?.Invoke(contextFactory, null)!;
                return new DisposeWrapper<DbContext>((DbContext)dbContext, true);
            }
            else if (dependencyResolver.TryResolve(dbContextType, keyedConnection, out var dbContext))
            {
                return new DisposeWrapper<DbContext>((DbContext)dbContext, false);
            }
            return null;
        }
    }
    public static DbContext? ResolveDbContext<TDbContext>(this IDependencyResolver dependencyResolver, string? keyedConnection = null) where TDbContext : DbContext
        => dependencyResolver.ResolveDbContext(typeof(TDbContext), keyedConnection);
    public static DbContext? ResolveDbContext(this IDependencyResolver dependencyResolver, Type? dbContextType, string? keyedConnection = null)
    {
        dbContextType ??= typeof(DbContext);
        var dbContextFactoryType = typeof(IDbContextFactory<>).MakeGenericType(dbContextType);
        if (keyedConnection == null)
        {
            if (dependencyResolver.TryResolve(dbContextFactoryType, out var dbContextFactory))
            {
                var contextFactory = (DbContext)dependencyResolver.Resolve(dbContextType);
                var dbContext = dbContextFactoryType.GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext))?.Invoke(contextFactory, null)!;
                return (DbContext)dbContext;
            }
            else if (dependencyResolver.TryResolve(dbContextType, out var dbContext))
            {
                return (DbContext)dbContext;
            }
            return null;
        }
        else
        {
            if (dependencyResolver.TryResolve(dbContextFactoryType, keyedConnection, out var dbContextFactory))
            {
                var contextFactory = (DbContext)dependencyResolver.Resolve(dbContextType);
                var dbContext = dbContextFactoryType.GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext))?.Invoke(contextFactory, null)!;
                return (DbContext)dbContext;
            }
            else if (dependencyResolver.TryResolve(dbContextType, keyedConnection, out var dbContext))
            {
                return (DbContext)dbContext;
            }
            return null;
        }
    }
}