using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Paillave.EntityFrameworkCoreExtension.Core;

namespace Paillave.EntityFrameworkCoreExtension.Searcher;

public abstract class SearchDescriptorBase<TEntity, TId>(DbContext dbContext) : ISearchDescriptor where TEntity : class
{
    // the actual regex (without c# escapings):
    // \.(?=(?:[^"]*"[^"]*")*(?![^"]*"))
    //private static Regex _regexSplitter = new Regex("\\.(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", RegexOptions.Singleline);
    protected DbContext DbContext { get; } = dbContext;
    private Func<IQueryable<TEntity>, IQueryable<TEntity>>? _include = null;

    protected abstract Expression<Func<TEntity, TId>> GetIdExpression { get; }
    public virtual string Name => typeof(TEntity).Name;
    public IDictionary<string, INavigationSelector> Navigations { get; } = new Dictionary<string, INavigationSelector>(StringComparer.InvariantCultureIgnoreCase);
    public IDictionary<string, IFieldSelector> Fields { get; } = new Dictionary<string, IFieldSelector>(StringComparer.InvariantCultureIgnoreCase);
    public IFieldSelector? DefaultValueProperty { get; private set; } = null;
    public SearchMetadata GetMetadata() => this.GetStructure(this.Name, this);
    private SearchMetadata GetStructure(string name, ISearchDescriptor descriptor)
        => new()
        {
            Name = name,
            Type = descriptor.Name,
            SubLevels = descriptor.Fields.Keys
                .Select(k => new SearchMetadata() { Name = k, Type = descriptor.Name })
                .Union(descriptor.Navigations.Select(v => GetStructure(v.Key, v.Value.ObjectDescriptor))
            ).ToList()
        };
    public SearchDescriptorBase<TEntity, TId> AddValue<TValue>(string name, bool isOnKey, Expression<Func<TEntity, TValue>> getValue, Func<string, TValue> convert)
    {
        var valueProperty = new FieldSelector<TEntity, TValue>(name, getValue, convert);
        this.Fields[name] = valueProperty;
        if (isOnKey) this.DefaultValueProperty = valueProperty;
        return this;
    }
    public SearchDescriptorBase<TEntity, TId> AddValue(string name, bool isOnKey, Expression<Func<TEntity, string>> getValue)
        => this.AddValue(name, isOnKey, getValue, i => i);
    public SearchDescriptorBase<TEntity, TId> SetIncludes(Func<IQueryable<TEntity>, IQueryable<TEntity>> include)
    {
        _include = include;
        return this;
    }
    public SearchDescriptorBase<TEntity, TId> AddNavigation<TTarget, TTargetId>(string name, SearchDescriptorBase<TTarget, TTargetId> levelDescriptor, Expression<Func<TEntity, TTarget>> getTargetExpression) where TTarget : class
    {
        var navigationProperty = new NavigationSelector<TEntity, TTarget, TTargetId>(name, getTargetExpression, levelDescriptor);
        this.Navigations[name] = navigationProperty;
        return this;
    }
    Expression ISearchDescriptor.GetFilterExpression(Dictionary<string, List<string>> filters)
        => this.GetFilterExpression(filters);
    public Expression<Func<TEntity, bool>> GetFilterExpression(Dictionary<string, List<string>> filters)
    {
        var entityParameterExpression = Expression.Parameter(typeof(TEntity), "entity");

        Expression? filterExpression = null;
        foreach (var filter in filters)
        {
            var subFilter = GetFilterExpressionBody(entityParameterExpression, filter.Key, filter.Value);
            if (filterExpression == null)
                filterExpression = subFilter;
            else
                filterExpression = Expression.AndAlso(filterExpression, subFilter);
        }
        if (filterExpression == null)
            return i => true;
        return (Expression<Func<TEntity, bool>>)Expression.Lambda(filterExpression, entityParameterExpression);
    }
    // Expression IObjectDescriptor.GetFilterExpression(string pathToProperty, IEnumerable<string> acceptedValues, IEnumerable<IObjectDescriptor> levelDescriptors)
    //     => this.GetFilterExpression(pathToProperty, acceptedValues, levelDescriptors);
    // private Type GetExpressionType(EXpression expression)
    // {
    //     switch (expression)
    //     {
    //         case ParameterExpression parameterExpression: return parameterExpression.Type;
    //         case InvocationExpression invocationExpression: return invocationExpression.Type;
    //     }
    // }
    private Expression GetFilterExpressionBody(ParameterExpression entityParameterExpression, string pathToProperty, IEnumerable<string> acceptedValues)
    {
        (var valueExpressionBody, var converter) = GetValueBodyExpression(entityParameterExpression, pathToProperty);
        var vals = CallMethod(nameof(ConvertAcceptedValues), new[] { valueExpressionBody.Type }, new object[] { acceptedValues, converter });
        var valueListExpression = Expression.Constant(vals);
        var queryableType = typeof(Enumerable);
        var containsMethod = queryableType.GetMethods()
            .First(i => i.Name == nameof(Queryable.Contains) && i.IsGenericMethod && i.GetGenericArguments().Length == 1 && i.GetParameters().Length == 2 && i.IsStatic);
        var typedContainsMethod = containsMethod.MakeGenericMethod(valueExpressionBody.Type);
        var containsCallExpression = Expression.Call(typedContainsMethod, valueListExpression, valueExpressionBody);
        return containsCallExpression;
    }
    // Expression GetValueExpression(string pathToProperty, List<IObjectDescriptor> levelDescriptors)
    //     => ;
    // Expression IObjectDescriptor.GetValueExpression(string pathToProperty, IEnumerable<IObjectDescriptor> levelDescriptors)
    //     => this.GetValueExpression<object>(pathToProperty, levelDescriptors);
    private TElement[] ConvertAcceptedValues<TElement>(IEnumerable<string> acceptedValues, Func<string, object> converter)
        => acceptedValues.Select(i => (TElement)converter(i)).ToArray();
    private List<string>? ParsePropertyPath(string line) => JsonSerializer.Deserialize<List<string>>(line);
    private (Expression BodyExpression, Func<string, object?> Converter) GetValueBodyExpression(ParameterExpression entityParameter, string pathToProperty)
    {
        if (string.IsNullOrWhiteSpace(pathToProperty))
            return (entityParameter, i => i);

        var pathSegments = this.ParsePropertyPath(pathToProperty) ?? new List<string>();

        // INavigationSelector navigationSelector = null;
        ISearchDescriptor objectDescriptor = this;
        Expression valueExpression = entityParameter;
        foreach (var pathSegment in pathSegments)
        {
            if (objectDescriptor.Navigations.TryGetValue(pathSegment, out var navigation))
            {
                valueExpression = GetExpressionBodyReferencingValue(navigation.GetTargetExpression, valueExpression) ?? throw new Exception("Navigation expression is null");
                // valueExpression = Expression.Invoke(navigation.GetTargetExpression, valueExpression);
                objectDescriptor = navigation.ObjectDescriptor;
            }
            else if (objectDescriptor.Fields.TryGetValue(pathSegment, out var field))
            {
                var valueExpressionBody = GetExpressionBodyReferencingValue(field.GetValueExpression, valueExpression);

                if (valueExpressionBody == null) throw new Exception("Value expression body is null");
                return (valueExpressionBody!, field.ConvertFromString);
                // return (Expression.Invoke(field.GetValueExpression, valueExpression), field.ConvertFromString);
            }
        }
        IFieldSelector defaultField = objectDescriptor.DefaultValueProperty ?? throw new Exception("Default value property is null");
        Expression expressionBodyReference = GetExpressionBodyReferencingValue(defaultField.GetValueExpression, valueExpression) ?? throw new Exception("Default value expression is null");
        return (expressionBodyReference, defaultField.ConvertFromString);
        // return (Expression.Invoke(defaultField.GetValueExpression, valueExpression), defaultField.ConvertFromString);
    }
    private Expression? GetExpressionBodyReferencingValue(LambdaExpression expression, Expression newParameterExpression)
    {
        var parameterToBeReplaced = expression.Parameters[0];
        var visitor = new ReplacementVisitor(parameterToBeReplaced, newParameterExpression);
        return visitor.Visit(expression.Body);
    }
    private LambdaExpression GetValueLambdaExpression(string pathToProperty)
    {
        var entityParameterExpression = Expression.Parameter(typeof(TEntity), "entity");
        (var valueExpressionBody, var converter) = GetValueBodyExpression(entityParameterExpression, pathToProperty);
        return Expression.Lambda(valueExpressionBody, entityParameterExpression);
    }
    private object? CallMethod(string methodName, Type[] genericParameters, object[] args)
    {
        // var methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        var method = typeof(SearchDescriptorBase<,>).MakeGenericType(typeof(TEntity), typeof(TId)).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (method == null)
            throw new Exception($"Method {methodName} not found");
        var groupQueryableMethodInfo = method.MakeGenericMethod(genericParameters);
        return groupQueryableMethodInfo.Invoke(this, args);
    }
    public Dictionary<GroupingValue, List<TEntity>> Search(Dictionary<string, List<string>> filters, string pathToGroupProperty)
    {
        var queryable = Search(filters);
        var groupingLambdaExpression = GetValueLambdaExpression(pathToGroupProperty);
        var groupingType = groupingLambdaExpression.ReturnType;
        var tmp = (Dictionary<GroupingValue, List<TEntity>>)(CallMethod(nameof(GroupQueryable), new[] { groupingLambdaExpression.ReturnType }, new object[] { queryable, groupingLambdaExpression }) ?? throw new Exception("GroupQueryable returned null"));
        return tmp;
        // var groupQueryableMethodInfo = this.GetType().GetMethod(nameof(GroupQueryable), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(groupingType);
        // return (List<List<TEntity>>)groupQueryableMethodInfo.Invoke(this, new object[] { queryable, groupingLambdaExpression });
    }
    private Dictionary<GroupingValue, List<TEntity>> GroupQueryable<TKey>(IQueryable<TEntity> queryable, Expression<Func<TEntity, TKey>> groupingExpression)
    {
        var entityParameterExpression = Expression.Parameter(typeof(TEntity), "entity");
        var keyedRowType = typeof(KeyedRow<>).MakeGenericType(typeof(TEntity), typeof(TId), typeof(TKey));
        var constructorInfo = keyedRowType.GetConstructor(new Type[] { }) ?? throw new Exception("Constructor not found");
        var callConstructor = Expression.New(constructorInfo);
        var keyedRowTypePropertyKey = keyedRowType.GetProperty(nameof(KeyedRow<int>.Key)) ?? throw new Exception("Property Key not found");
        var keyedRowTypePropertyRow = keyedRowType.GetProperty(nameof(KeyedRow<int>.Row)) ?? throw new Exception("Property Row not found");
        var expressionBody = GetExpressionBodyReferencingValue(groupingExpression, entityParameterExpression)
            ?? throw new Exception("Grouping expression body is null");
        var initializedObject = Expression.MemberInit(callConstructor,
            Expression.Bind(keyedRowTypePropertyKey, expressionBody),
            Expression.Bind(keyedRowTypePropertyRow, entityParameterExpression));
        var getGroupingKeyLambdaExpression = (Expression<Func<TEntity, KeyedRow<TKey>>>)Expression.Lambda(initializedObject, entityParameterExpression);
        return queryable.Select(getGroupingKeyLambdaExpression).ToList().GroupBy(i => i.Key).ToDictionary(i => new GroupingValue(i.Key), i => i.Select(j => j.Row).ToList());
    }
    // private Expression<Func<TEntity, KeyedRow<TKey, TEntity>> 
    public virtual IQueryable<TEntity> Search(Dictionary<string, List<string>> filters)
    {
        var queryable = this.GetQueryable();
        if (filters == null || filters.Count == 0) return queryable;
        return queryable.Where(GetFilterExpression(filters));
    }
    public virtual List<TId> SearchIds(Dictionary<string, List<string>> filters)
        => this.Search(filters).Select(this.GetIdExpression).ToList();
    public virtual Dictionary<GroupingValue, List<(TId id, string label)>> SearchIds(Dictionary<string, List<string>> filters, string pathToGroupProperty)
    {
        Func<TEntity, TId> getId = this.GetIdExpression.Compile() ?? throw new Exception("GetIdExpression is null");
        var defaultValue = this.DefaultValueProperty?.GetValueExpression.Compile() as Func<TEntity, object>;
        return this.Search(filters, pathToGroupProperty).ToDictionary(i => i.Key, i => i.Value.Select(j =>
        {
            var id = getId(j);
            string label = id?.ToString() ?? string.Empty;
            if (defaultValue == null) return (id, label);
            try { label = defaultValue(j)?.ToString() ?? string.Empty; }
            catch { }
            (TId id, string label) searchCriteria = (id, label);
            return searchCriteria;
        }).ToList());
    }
    protected virtual IQueryable<TEntity> GetQueryable() => Include(this.DbContext.Set<TEntity>().AsQueryable());

    protected IQueryable<TEntity> Include(IQueryable<TEntity> query)
    {
        if (_include == null)
            return query;
        return _include(query);
    }
    private class KeyedRow<TKey>
    {
        public required TKey Key { get; set; }
        public required TEntity Row { get; set; }
    }
}
