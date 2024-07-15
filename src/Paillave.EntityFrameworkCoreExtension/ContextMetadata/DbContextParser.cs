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

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class DbContextParser<TCtx> where TCtx : DbContext
    {
        public ModelStructure GetModelStructure(TCtx dbContext)
        {
            var modelStructure = new ModelStructure();
            var model = dbContext.GetService<IDesignTimeModel>().Model;
            // var model = dbContext.Model;
            var entityTypes = model.GetEntityTypes().OrderBy(i => i.Name).ToList();

            modelStructure.Entities = entityTypes.Select(CreateEntitySummary).ToDictionary(i => i.Name);
            modelStructure.Links = entityTypes
                .SelectMany(i => i.GetNavigations().Where(navigation => navigation.DeclaringType.ClrType.Name == i.ClrType.Name).Select(n => CreateLinkSummary(i, n)))
                .Union(entityTypes.Where(i => i.BaseType != null).Select(i => CreateInheritLinkSummary(i, i.BaseType))).ToList();
            return modelStructure;
        }
        public static LinkSummary CreateLinkSummary(IEntityType entityType, INavigation navigation)
        {
            var from = entityType.GetEntityEssentials();
            var to = navigation.TargetEntityType.GetEntityEssentials();

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
                Required = navigation.ForeignKey.IsRequired
            };
        }
        public static LinkSummary CreateInheritLinkSummary(IEntityType from, IEntityType to)
        {
            var fromMapping = from.GetEntityEssentials();
            var toMapping = to.GetEntityEssentials();
            return new LinkSummary
            {
                FromName = fromMapping.Name,
                FromSchema = fromMapping.Schema,
                From = $"{fromMapping.Schema}.{fromMapping.Name}",
                ToName = toMapping.Name,
                ToSchema = toMapping.Schema,
                To = $"{toMapping.Schema}.{toMapping.Name}",
                Type = LinkType.Inherits
            };
        }
        public static EntitySummary CreateEntitySummary(IEntityType entityType)
        {
            var mapping = entityType.GetEntityEssentials();
            mapping.Comment = entityType.GetComment();
            var storeObject = StoreObjectIdentifier.Create(entityType, mapping.IsView ? StoreObjectType.View : StoreObjectType.Table).GetValueOrDefault();
            mapping.Properties = entityType.GetDeclaredProperties().Where(i => !i.IsShadowProperty()).Select(i => CreatePropertySummary(i, storeObject)).ToList();
            return mapping;
        }
        public static PropertySummary CreatePropertySummary(IProperty property, StoreObjectIdentifier storeObject) =>
            new PropertySummary
            {
                Name = property.GetColumnName(storeObject),
                Type = GetTypeLabel(property.ClrType),
                IsForeignKey = property.IsForeignKey(),
                IsKey = property.IsKey(),
                IsNullable = property.IsNullable,
                MaxLength = property.GetMaxLength()
            };
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
            return File.Exists(xmlDocumentationPath) ? XDocument.Load(xmlDocumentationPath) : null;
        }

        public ModelStructure GetModelStructure()
        {
            var modelStructure = new ModelStructure();
            var dbContext = CreateDbContextInstance(_assembly, _args);
            var entityTypes = dbContext.Model.GetEntityTypes().ToList();

            if (_xmlDocumentation != null)
            {
                var members = _xmlDocumentation.Descendants().Elements("member");
                modelStructure.Comments = members
                   .Where(i => i.Attribute("name").Value.StartsWith("T:"))
                   .Select(i => new
                   {
                       TypeName = i.Attribute("name").Value.Split('.').LastOrDefault(),
                       Comment = i.Element("summary").Value
                   })
                   .ToDictionary(i => i.TypeName, i => i.Comment);
            }
            modelStructure.Entities = entityTypes.Select(CreateEntitySummary).ToDictionary(i => i.Name);
            modelStructure.Links = entityTypes
                .SelectMany(i => i.GetNavigations().Where(navigation => navigation.DeclaringType.ClrType.Name == i.ClrType.Name).Select(n => CreateLinkSummary(i, n)))
                .Union(entityTypes.Where(i => i.BaseType != null).Select(i => CreateInheritLinkSummary(i, i.BaseType))).ToList();
            return modelStructure;
        }
        public static LinkSummary CreateLinkSummary(IEntityType entityType, INavigation navigation)
        {
            var from = entityType.GetEntityEssentials();
            var to = navigation.TargetEntityType.GetEntityEssentials();
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
                Required = navigation.ForeignKey.IsRequired
            };
        }
        public static LinkSummary CreateInheritLinkSummary(IEntityType from, IEntityType to)
        {
            var fromSummary = from.GetEntityEssentials();
            var toSummary = to.GetEntityEssentials();
            return new LinkSummary
            {
                From = $"{fromSummary.Schema}.{fromSummary.Name}",
                FromSchema = fromSummary.Schema,
                FromName = fromSummary.Name,
                To = $"{toSummary.Schema}.{toSummary.Name}",
                ToSchema = toSummary.Schema,
                ToName = toSummary.Name,
                Type = LinkType.Inherits
            };
        }
        public static EntitySummary CreateEntitySummary(IEntityType entityType)
        {
            var storeObject = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table).GetValueOrDefault();
            return new EntitySummary
            {
                IsAbstract = entityType.IsAbstract(),
                IsView = entityType.IsTableExcludedFromMigrations(),
                Name = entityType.ClrType.Name,
                Schema = entityType.GetSchema(),
                Properties = entityType.GetDeclaredProperties().Where(i => !i.IsShadowProperty()).Select(i => CreatePropertySummary(i, storeObject)).ToList(),
                Comment = entityType.GetComment()
            };
        }
        public static PropertySummary CreatePropertySummary(IProperty property, StoreObjectIdentifier storeObject) =>
            new PropertySummary
            {
                Name = property.GetColumnName(storeObject),
                Type = GetTypeLabel(property.ClrType),
                IsForeignKey = property.IsForeignKey(),
                IsKey = property.IsKey(),
                IsNullable = property.IsNullable,
                MaxLength = property.GetMaxLength()
            };
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
                return e.Types.Where(t => t != null);
            }
        }
        private static DbContext CreateDbContextInstance(Assembly assembly, string[] args)
        {
            var allTypes = GetTypes(assembly);
            var dbContextType = allTypes.FirstOrDefault(i => typeof(DbContext).IsAssignableFrom(i));
            if (dbContextType == null) return null;
            if (dbContextType.GetConstructor(new Type[] { }) == null)
            {
                var databaseContextFactoryType = allTypes.FirstOrDefault(IsDatabaseContextFactoryType);
                if (databaseContextFactoryType == null) return null;
                var designTimeDbContextFactory = Activator.CreateInstance(databaseContextFactoryType);
                var methodInfo = databaseContextFactoryType.GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>.CreateDbContext));
                return methodInfo.Invoke(designTimeDbContextFactory, new object[] { args }) as DbContext;
            }
            else
            {
                return Activator.CreateInstance(dbContextType) as DbContext;
            }
        }
        private static bool IsDatabaseContextFactoryType(Type type)
            => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>));
    }
    public static class EntityTypeEx
    {
        public static EntitySummary GetEntityEssentials(this IEntityType entityType)
        {
            ITableMappingBase viewMapping = entityType.GetViewMappings().FirstOrDefault();
            ITableMappingBase tableMapping = entityType.GetTableMappings().FirstOrDefault();
            var isView = viewMapping != null;
            var schemaName = isView ? entityType.GetViewSchema() : entityType.GetSchema();
            var tableName = isView ? entityType.GetViewName() : entityType.GetTableName();
            return new EntitySummary
            {
                IsAbstract = entityType.IsAbstract(),
                IsView = isView,
                Name = entityType.ClrType.Name,
                Schema = schemaName,
                TargetName = tableName
            };
        }
    }
}