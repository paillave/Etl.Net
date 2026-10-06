using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata;

public class DbContextParser<TCtx> where TCtx : DbContext
{
    public ModelStructure GetModelStructure(TCtx dbContext)
    {
        // var modelStructure = new ModelStructure();
        var model = dbContext.GetService<IDesignTimeModel>().Model;
        // var model = dbContext.Model;
        var entityTypes = model.GetEntityTypes().Where(i => !i.IsPropertyBag && !i.IsMappedToJson()).OrderBy(i => i.Name).ToList();

        var modelStructureEntities = entityTypes.Select(CreateEntitySummary).GroupBy(i => i.Name).ToDictionary(g => g.Key, g => g.First());
        var modelStructureLinks = entityTypes
            .SelectMany(i => i.GetNavigations().Where(navigation => navigation.DeclaringType == i && !navigation.TargetEntityType.IsMappedToJson()).Select(n => CreateLinkSummary(i, n)))
            .Union(entityTypes.SelectMany(i => i.GetSkipNavigations().Where(navigation => navigation.DeclaringType == i && !navigation.TargetEntityType.IsMappedToJson()).Select(n => CreateSkipLinkSummary(i, n))))
            .Union(entityTypes.Where(i => i.BaseType != null).Select(i => CreateInheritLinkSummary(i, i.BaseType!))).ToList();
        return new ModelStructure
        {
            Entities = modelStructureEntities,
            Links = modelStructureLinks
        };
    }
    public static LinkSummary CreateLinkSummary(IEntityType entityType, INavigation navigation)
    {
        var from = entityType.GetEntityEssentials();
        from.Name = entityType.GetSummaryName();
        var to = navigation.TargetEntityType.GetEntityEssentials();
        to.Name = navigation.TargetEntityType.GetSummaryName();

        return new LinkSummary
        {
            FromName = from.Name,
            FromSchema = from.Schema,
            From = $"{from.Schema}.{from.Name}",
            ToName = to.Name,
            ToSchema = to.Schema,
            To = $"{to.Schema}.{to.Name}",
            Name = navigation.Name,
            Type = navigation.IsCollection ? LinkType.Aggregates : LinkType.References,
            Required = navigation.ForeignKey.IsRequired,
            CascadeDelete = navigation.ForeignKey.DeleteBehavior == DeleteBehavior.Cascade,
            ForeignKeyProperties = navigation.ForeignKey.Properties.Select(fkProperty => fkProperty.Name).ToList()
        };
    }
    /// <summary>Same as <see cref="CreateLinkSummary(IEntityType, INavigation)"/>, but for a many-to-many
    /// relationship (<see cref="ISkipNavigation"/>), which <see cref="IEntityType.GetNavigations"/> never
    /// returns — it has to be fetched separately via <see cref="IEntityType.GetSkipNavigations"/>.</summary>
    public static LinkSummary CreateSkipLinkSummary(IEntityType entityType, ISkipNavigation navigation)
    {
        var from = entityType.GetEntityEssentials();
        from.Name = entityType.GetSummaryName();
        var to = navigation.TargetEntityType.GetEntityEssentials();
        to.Name = navigation.TargetEntityType.GetSummaryName();

        return new LinkSummary
        {
            FromName = from.Name,
            FromSchema = from.Schema,
            From = $"{from.Schema}.{from.Name}",
            ToName = to.Name,
            ToSchema = to.Schema,
            To = $"{to.Schema}.{to.Name}",
            Name = navigation.Name,
            Type = LinkType.Aggregates,
            Required = false,
            CascadeDelete = false,
            ForeignKeyProperties = []
        };
    }
    public static LinkSummary CreateInheritLinkSummary(IEntityType from, IEntityType to)
    {
        var fromMapping = from.GetEntityEssentials();
        fromMapping.Name = from.GetSummaryName();
        var toMapping = to.GetEntityEssentials();
        toMapping.Name = to.GetSummaryName();
        return new LinkSummary
        {
            FromName = fromMapping.Name,
            FromSchema = fromMapping.Schema,
            From = $"{fromMapping.Schema}.{fromMapping.Name}",
            ToName = toMapping.Name,
            ToSchema = toMapping.Schema,
            To = $"{toMapping.Schema}.{toMapping.Name}",
            Type = LinkType.Inherits,
            Required = false,
            CascadeDelete = false,
            ForeignKeyProperties = []
        };
    }
    public static EntitySummary CreateEntitySummary(IEntityType entityType)
    {
        var mapping = entityType.GetEntityEssentials();
        mapping.Name = entityType.GetSummaryName();
        mapping.Comment = entityType.GetComment();
        var storeObject = StoreObjectIdentifier.Create(entityType, mapping.IsView ? StoreObjectType.View : StoreObjectType.Table).GetValueOrDefault();
        mapping.Properties = entityType.GetDeclaredProperties().Where(i => !i.IsShadowProperty()).Select(i => CreatePropertySummary(i, storeObject)).ToList();
        return mapping;
    }
    public static PropertySummary CreatePropertySummary(IProperty property, StoreObjectIdentifier storeObject)
    {
        return new PropertySummary
        {
            Name = (storeObject.StoreObjectType == StoreObjectType.Table || storeObject.StoreObjectType == StoreObjectType.View ? property.GetColumnName(storeObject) : null) ?? property.Name,
            ClrName = property.Name,
            Type = GetTypeLabel(property.ClrType),
            IsForeignKey = property.IsForeignKey(),
            IsKey = property.IsKey(),
            IsNullable = property.IsNullable,
            MaxLength = property.GetMaxLength()
        };
    }
    private static string GetTypeLabel(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType == null ? type.Name : underlyingType.Name;
    }
}

public class DbContextParser(Assembly assembly, XDocument xmlDocumentation, string[] args)
{
    private readonly Assembly _assembly = assembly;
    private readonly XDocument _xmlDocumentation = xmlDocumentation;
    private readonly string[] _args = args;

    public DbContextParser(string assemblyPath, string[] args) : this(Assembly.LoadFrom(assemblyPath), GetXmlDocumentation(assemblyPath), args)
    {
    }

    private static XDocument GetXmlDocumentation(string assemblyPath)
    {
        var xmlDocumentationPath = Path.ChangeExtension(assemblyPath, "xml");
        return File.Exists(xmlDocumentationPath) ? XDocument.Load(xmlDocumentationPath) : throw new InvalidOperationException("Xml documentation file not found");
    }

    public ModelStructure GetModelStructure()
    {
        // var modelStructure = new ModelStructure();
        var dbContext = CreateDbContextInstance(_assembly, _args);
        var entityTypes = dbContext.Model.GetEntityTypes().Where(i => !i.IsPropertyBag && !i.IsMappedToJson()).ToList();
        Dictionary<string, string>? modelStructureComments = null;
        if (_xmlDocumentation != null)
        {
            var members = _xmlDocumentation.Descendants().Elements("member");
            modelStructureComments = members
               .Where(i => i.Attribute("name")?.Value?.StartsWith("T:") ?? false)
               .Select(i => new
               {
                   TypeName = i.Attribute("name")?.Value.Split('.').LastOrDefault(),
                   Comment = i.Element("summary")?.Value
               })
               .Where(i => i.Comment != null && i.TypeName != null)
               .ToDictionary(i => i.TypeName!, i => i.Comment!);
        }
        var modelStructureEntities = entityTypes.Select(CreateEntitySummary).GroupBy(i => i.Name).ToDictionary(g => g.Key, g => g.First());
        var modelStructureLinks = entityTypes
            .SelectMany(i => i.GetNavigations().Where(navigation => navigation.DeclaringType == i && !navigation.TargetEntityType.IsMappedToJson()).Select(n => CreateLinkSummary(i, n)))
            .Union(entityTypes.SelectMany(i => i.GetSkipNavigations().Where(navigation => navigation.DeclaringType == i && !navigation.TargetEntityType.IsMappedToJson()).Select(n => CreateSkipLinkSummary(i, n))))
            .Union(entityTypes.Where(i => i.BaseType != null).Select(i => CreateInheritLinkSummary(i, i.BaseType!))).ToList();
        return new ModelStructure
        {
            Entities = modelStructureEntities,
            Links = modelStructureLinks,
            Comments = modelStructureComments
        };
    }
    public static LinkSummary CreateLinkSummary(IEntityType entityType, INavigation navigation)
    {
        var from = entityType.GetEntityEssentials();
        from.Name = entityType.GetSummaryName();
        var to = navigation.TargetEntityType.GetEntityEssentials();
        to.Name = navigation.TargetEntityType.GetSummaryName();
        return new LinkSummary
        {
            From = $"{from.Schema}.{from.Name}",
            FromSchema = from.Schema,
            FromName = from.Name,
            To = $"{to.Schema}.{to.Name}",
            ToSchema = to.Schema,
            ToName = to.Name,
            Name = navigation.Name,
            Type = navigation.IsCollection ? LinkType.Aggregates : LinkType.References,
            Required = navigation.ForeignKey.IsRequired,
            CascadeDelete = navigation.ForeignKey.DeleteBehavior == DeleteBehavior.Cascade,
            ForeignKeyProperties = navigation.ForeignKey.Properties.Select(fkProperty => fkProperty.Name).ToList()
        };
    }
    /// <summary>Same as <see cref="CreateLinkSummary(IEntityType, INavigation)"/>, but for a many-to-many
    /// relationship (<see cref="ISkipNavigation"/>), which <see cref="IEntityType.GetNavigations"/> never
    /// returns — it has to be fetched separately via <see cref="IEntityType.GetSkipNavigations"/>.</summary>
    public static LinkSummary CreateSkipLinkSummary(IEntityType entityType, ISkipNavigation navigation)
    {
        var from = entityType.GetEntityEssentials();
        from.Name = entityType.GetSummaryName();
        var to = navigation.TargetEntityType.GetEntityEssentials();
        to.Name = navigation.TargetEntityType.GetSummaryName();
        return new LinkSummary
        {
            From = $"{from.Schema}.{from.Name}",
            FromSchema = from.Schema,
            FromName = from.Name,
            To = $"{to.Schema}.{to.Name}",
            ToSchema = to.Schema,
            ToName = to.Name,
            Name = navigation.Name,
            Type = LinkType.Aggregates,
            Required = false,
            CascadeDelete = false,
            ForeignKeyProperties = []
        };
    }
    public static LinkSummary CreateInheritLinkSummary(IEntityType from, IEntityType to)
    {
        var fromSummary = from.GetEntityEssentials();
        fromSummary.Name = from.GetSummaryName();
        var toSummary = to.GetEntityEssentials();
        toSummary.Name = to.GetSummaryName();
        return new LinkSummary
        {
            From = $"{fromSummary.Schema}.{fromSummary.Name}",
            FromSchema = fromSummary.Schema,
            FromName = fromSummary.Name,
            To = $"{toSummary.Schema}.{toSummary.Name}",
            ToSchema = toSummary.Schema,
            ToName = toSummary.Name,
            Type = LinkType.Inherits,
            Required = false,
            CascadeDelete = false,
            ForeignKeyProperties = []
        };
    }
    public static EntitySummary CreateEntitySummary(IEntityType entityType)
    {
        var storeObject = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table).GetValueOrDefault();
        return new EntitySummary
        {
            IsAbstract = entityType.IsAbstract(),
            IsView = entityType.IsTableExcludedFromMigrations(),
            Name = entityType.GetSummaryName(),
            Schema = entityType.GetSchema(),
            Properties = entityType.GetDeclaredProperties().Where(i => !i.IsShadowProperty()).Select(i => CreatePropertySummary(i, storeObject)).ToList(),
            Comment = entityType.GetComment()
        };
    }
    public static PropertySummary CreatePropertySummary(IProperty property, StoreObjectIdentifier storeObject)
    {
        return new PropertySummary
        {
            Name = (storeObject.StoreObjectType == StoreObjectType.Table || storeObject.StoreObjectType == StoreObjectType.View ? property.GetColumnName(storeObject) : null) ?? property.Name,
            ClrName = property.Name,
            Type = GetTypeLabel(property.ClrType),
            IsForeignKey = property.IsForeignKey(),
            IsKey = property.IsKey(),
            IsNullable = property.IsNullable,
            MaxLength = property.GetMaxLength()
        };
    }
    private static string GetTypeLabel(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType == null ? type.Name : underlyingType.Name;
    }
    private static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null).Cast<Type>();
        }
    }
    private static DbContext CreateDbContextInstance(Assembly assembly, string[] args)
    {
        var allTypes = GetTypes(assembly);
        var dbContextType = allTypes.FirstOrDefault(i => typeof(DbContext).IsAssignableFrom(i));
        if (dbContextType == null) throw new InvalidOperationException("DbContext type not found");
        if (dbContextType.GetConstructor(new Type[] { }) == null)
        {
            var databaseContextFactoryType = allTypes.FirstOrDefault(IsDatabaseContextFactoryType);
            if (databaseContextFactoryType == null) throw new InvalidOperationException("IDesignTimeDbContextFactory not found");
            var designTimeDbContextFactory = Activator.CreateInstance(databaseContextFactoryType);
            var methodInfo = databaseContextFactoryType.GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>.CreateDbContext));
            if (methodInfo == null) throw new InvalidOperationException("CreateDbContext method not found");
            return methodInfo.Invoke(designTimeDbContextFactory, new object[] { args }) as DbContext ?? throw new InvalidOperationException("CreateDbContext method returned null");
        }
        else
        {
            return Activator.CreateInstance(dbContextType) as DbContext ?? throw new InvalidOperationException("DbContext instance could not be created");
        }
    }
    private static bool IsDatabaseContextFactoryType(Type type)
        => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>));
}
public static class EntityTypeEx
{
    /// <summary>CLR type name, unless several entity types of the model share that CLR type (typically an owned type
    /// attached to several owners), in which case the unique EF entity type name is used.</summary>
    public static string GetSummaryName(this IEntityType entityType)
        => entityType.Model.GetEntityTypes().Count(i => i.ClrType == entityType.ClrType) > 1 ? entityType.Name : entityType.ClrType.Name;

    public static EntitySummary GetEntityEssentials(this IEntityType entityType)
    {
        ITableMappingBase? viewMapping = entityType.GetViewMappings().FirstOrDefault();
        // ITableMappingBase? tableMapping = entityType.GetTableMappings().FirstOrDefault();
        var isView = viewMapping != null;
        var schemaName = isView ? entityType.GetViewSchema() : entityType.GetSchema();
        var tableName = isView ? entityType.GetViewName() : entityType.GetTableName();
        return new EntitySummary
        {
            IsAbstract = entityType.IsAbstract(),
            IsView = isView,
            Name = entityType.ClrType.Name,
            Schema = schemaName,
            TargetName = tableName,
            Properties = new List<PropertySummary>()
        };
    }
}