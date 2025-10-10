using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.GraphApi.Provider;

public static class ExpressionEx
{
    public static string ToODAtaString<T, TRes>(this Expression<Func<T, TRes>> expression) where T : class
        => new ExpressionWriter(new ODataExpressionConverterSettings()).Write(expression);
    public static string[] ToODAtaProjections<T, TRes>(this Expression<Func<T, TRes>> expression) where T : class
        => ProjectionProcessor<T>.ToString(expression);
}
public class ODataExpression<T> where T : class
{
    public static string ToString(Expression<Func<T, bool>> expression) => expression.ToODAtaString();
    public static string[] ToString<TRes>(Expression<Func<T, TRes>> expression) where TRes : class => expression.ToODAtaProjections();
}

public class ProjectionProcessor<T>
{
    public static string[] ToString<TRes>(Expression<Func<T, TRes>> expression)
    {
        ProjectionVisitor vis = new();
        vis.Visit(expression);
        return vis.Projections;

    }
}
public class ProjectionVisitor : ExpressionVisitor
{
    public string[] Projections { get; private set; } = [];
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        NewInstanceVisitor vis = new();
        vis.Visit(node.Body);
        this.Projections = [.. vis.Projections];
        return base.VisitLambda(node);
    }
}
public class NewInstanceVisitor : ExpressionVisitor
{
    public List<string> Projections { get; private set; } = new List<string>();

    protected override Expression VisitNew(NewExpression node)
    {
        if (node.Members != null)
        {
            Projections = node.Members.Select(i => i.Name).ToList();
        }
        return base.VisitNew(node);
    }
    protected override Expression VisitMember(MemberExpression node)
    {
        if (Projections.Count == 0)
        {
            Projections.Add(node.Member.Name);
        }
        return base.VisitMember(node);
    }
}
