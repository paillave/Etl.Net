using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Paillave.EntityFrameworkCoreExtension;

public static class EntityTypeBuilderEx
{
    public static EntityTypeBuilder<TEntity> ToTable<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder) where TEntity : class
    {
        var type = typeof(TEntity);
        var tableName = type.Name;
        var schemaName = type.Namespace?.Split('.').Last();
        return entityTypeBuilder.ToTable(tableName, schemaName);
    }
    public static OwnedNavigationBuilder<TFrom, TTo> ToTable<TFrom, TTo>(this OwnedNavigationBuilder<TFrom, TTo> entityTypeBuilder) where TFrom : class where TTo : class
    {
        var type = typeof(TTo);
        var tableName = type.Name;
        var schemaName = type.Namespace?.Split('.').Last();
        return entityTypeBuilder.ToTable(tableName, schemaName);
    }
}