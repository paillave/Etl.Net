using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class DbContextParser<TCtx> where TCtx : DbContext
    {
        public ModelStructure GetModelStructure(TCtx dbContext)
        {
            var modelStructure = new ModelStructure();
            var entityTypes = dbContext.Model.GetEntityTypes().ToList();

            modelStructure.Entities = entityTypes.Select(CreateEntitySummary).ToDictionary(i => i.Name);
            modelStructure.Links = entityTypes
                .SelectMany(i => i.GetNavigations().Where(navigation => navigation.DeclaringType.ClrType.Name == i.ClrType.Name).Select(n => CreateLinkSummary(i, n)))
                .Union(entityTypes.Where(i => i.BaseType != null).Select(i => CreateInheritLinkSummary(i, i.BaseType))).ToList();
            return modelStructure;
        }
        public static LinkSummary CreateLinkSummary(IEntityType entityType, INavigation navigation)
        {
            return new LinkSummary
            {
                FromName = entityType.ClrType.Name,
                FromSchema = entityType.GetSchema(),
                From = $"{entityType.GetSchema()}.{entityType.ClrType.Name}",
                ToName = navigation.GetTargetType().ClrType.Name,
                ToSchema = navigation.GetTargetType().GetSchema(),
                To = $"{navigation.GetTargetType().GetSchema()}.{navigation.GetTargetType().ClrType.Name}",
                Name = navigation.Name,
                Type = navigation.IsCollection() ? LinkType.Aggregates : LinkType.References,
                Required = navigation.ForeignKey.IsRequired
            };
        }
        public static LinkSummary CreateInheritLinkSummary(IEntityType from, IEntityType to)
        {
            return new LinkSummary
            {
                FromName = from.ClrType.Name,
                FromSchema = from.GetSchema(),
                From = $"{from.GetSchema()}.{from.ClrType.Name}",
                ToName = to.ClrType.Name,
                ToSchema = to.GetSchema(),
                To = $"{to.GetSchema()}.{to.ClrType.Name}",
                Type = LinkType.Inherits
            };
        }
        public static EntitySummary CreateEntitySummary(IEntityType entityType)
        {
            return new EntitySummary
            {
                IsAbstract = entityType.IsAbstract(),
                IsView = entityType.IsIgnoredByMigrations(),
                Name = entityType.ClrType.Name,
                Schema = entityType.GetSchema(),
                Properties = entityType.GetDeclaredProperties().Where(i => !i.IsShadowProperty()).Select(CreatePropertySummary).ToList(),
                Comment = entityType.GetComment()
            };
        }
        public static PropertySummary CreatePropertySummary(IProperty property)
        {
            return new PropertySummary
            {
                Name = property.GetColumnName(),
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

    public class DbContextParser
    {
        private readonly Assembly _assembly;
        private readonly XDocument _xmlDocumentation;
        private readonly string[] _args;

        public DbContextParser(string assemblyPath, string[] args) : this(Assembly.LoadFrom(assemblyPath), GetXmlDocumentation(assemblyPath), args)
        {
        }
        public DbContextParser(Assembly assembly, XDocument xmlDocumentation, string[] args)
        {
            this._args = args;
            this._xmlDocumentation = xmlDocumentation;
            this._assembly = assembly;
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
            return new LinkSummary
            {
                From = $"{entityType.GetSchema()}.{entityType.ClrType.Name}",
                FromSchema = entityType.GetSchema(),
                FromName = entityType.ClrType.Name,
                To = $"{navigation.GetTargetType().GetSchema()}.{navigation.GetTargetType().ClrType.Name}",
                ToSchema = navigation.GetTargetType().GetSchema(),
                ToName = navigation.GetTargetType().ClrType.Name,
                Name = navigation.Name,
                Type = navigation.IsCollection() ? LinkType.Aggregates : LinkType.References,
                Required = navigation.ForeignKey.IsRequired
            };
        }
        public static LinkSummary CreateInheritLinkSummary(IEntityType from, IEntityType to)
        {
            return new LinkSummary
            {
                From = $"{from.GetSchema()}.{from.ClrType.Name}",
                FromSchema = from.GetSchema(),
                FromName = from.ClrType.Name,
                To = $"{to.GetSchema()}.{to.ClrType.Name}",
                ToSchema = to.GetSchema(),
                ToName = to.ClrType.Name,
                Type = LinkType.Inherits
            };
        }
        public static EntitySummary CreateEntitySummary(IEntityType entityType)
        {
            return new EntitySummary
            {
                IsAbstract = entityType.IsAbstract(),
                IsView = entityType.IsIgnoredByMigrations(),
                Name = entityType.ClrType.Name,
                Schema = entityType.GetSchema(),
                Properties = entityType.GetDeclaredProperties().Where(i => !i.IsShadowProperty()).Select(CreatePropertySummary).ToList(),
                Comment = entityType.GetComment()
            };
        }
        public static PropertySummary CreatePropertySummary(IProperty property)
        {
            return new PropertySummary
            {
                Name = property.GetColumnName(),
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
}