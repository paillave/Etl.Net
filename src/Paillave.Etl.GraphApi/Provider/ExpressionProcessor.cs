using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.GraphApi.Provider;

public interface IExpressionProcessor
{
    object ProcessMethodCall<T>(MethodCallExpression methodCall, ParameterBuilder builder, Func<ParameterBuilder, IEnumerable<T>> resultLoader, Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader);
}
public class ExpressionProcessor(ODataExpressionConverterSettings settings, IExpressionWriter writer) : IExpressionProcessor
{
    private readonly IExpressionWriter _writer = writer;
    readonly ODataExpressionConverterSettings settings = settings;

    public ExpressionProcessor(ODataExpressionConverterSettings settings) : this(settings, new ExpressionWriter(settings))
    { }

    public object ProcessMethodCall<T>(MethodCallExpression methodCall, ParameterBuilder builder, Func<ParameterBuilder, IEnumerable<T>> resultLoader, Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {
        if (methodCall == null)
        {
            return null;
        }

        var method = methodCall.Method.Name;

        switch (method)
        {
            case "First":
            case "FirstOrDefault":
                builder.TakeParameter = "1";
                return methodCall.Arguments.Count >= 2
                            ? GetMethodResult(methodCall, builder, resultLoader, intermediateResultLoader)
                            : GetResult(methodCall, builder, resultLoader, intermediateResultLoader);

            case "Single":
            case "SingleOrDefault":
                builder.TakeParameter = "2";
                return methodCall.Arguments.Count >= 2
                        ? GetMethodResult(methodCall, builder, resultLoader, intermediateResultLoader)
                        : GetResult(methodCall, builder, resultLoader, intermediateResultLoader);
            case "Last":
            case "LastOrDefault":
                return methodCall.Arguments.Count >= 2
                        ? GetMethodResult(methodCall, builder, resultLoader, intermediateResultLoader)
                        : GetResult(methodCall, builder, resultLoader, intermediateResultLoader);
            case "Count":
            case "LongCount":
                builder.IncludeCount = true;
                return methodCall.Arguments.Count >= 2
                        ? GetMethodResult(methodCall, builder, resultLoader, intermediateResultLoader)
                        : GetResult(methodCall, builder, resultLoader, intermediateResultLoader);
            case "Where":
                {
                    var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
                    if (result != null)
                    {
                        return InvokeEager(methodCall, result);
                    }

                    var newFilter = _writer.Write(methodCall.Arguments[1]);

                    builder.FilterParameter = string.IsNullOrWhiteSpace(builder.FilterParameter)
                                                ? newFilter
                                                : string.Format("({0}) and ({1})", builder.FilterParameter, newFilter);
                }

                break;
            case "Select":
                {
                    var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
                    if (result != null)
                    {
                        return InvokeEager(methodCall, result);
                    }

                    if (!string.IsNullOrWhiteSpace(builder.SelectParameter))
                    {
                        return ExecuteMethod(methodCall, builder, resultLoader, intermediateResultLoader);
                    }

                    var unaryExpression = methodCall.Arguments[1] as UnaryExpression;
                    if (unaryExpression != null)
                    {
                        var lambdaExpression = unaryExpression.Operand as LambdaExpression;
                        if (lambdaExpression != null)
                        {
                            return ResolveProjection(builder, lambdaExpression);
                        }
                    }
                }

                break;
            case "OrderBy":
            case "ThenBy":
                {
                    var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
                    if (result != null)
                    {
                        return InvokeEager(methodCall, result);
                    }

                    var item = _writer.Write(methodCall.Arguments[1]);
                    builder.OrderByParameter.Add(item);
                }

                break;
            case "OrderByDescending":
            case "ThenByDescending":
                {
                    var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
                    if (result != null)
                    {
                        return InvokeEager(methodCall, result);
                    }

                    var visit = _writer.Write(methodCall.Arguments[1]);
                    builder.OrderByParameter.Add(visit + " desc");
                }

                break;
            case "Take":
                {
                    var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
                    if (result != null)
                    {
                        return InvokeEager(methodCall, result);
                    }

                    builder.TakeParameter = _writer.Write(methodCall.Arguments[1]);
                }

                break;
            case "Skip":
                {
                    var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
                    if (result != null)
                    {
                        return InvokeEager(methodCall, result);
                    }

                    builder.SkipParameter = _writer.Write(methodCall.Arguments[1]);
                }

                break;
            case "Expand":
                {
                    var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
                    if (result != null)
                    {
                        return InvokeEager(methodCall, result);
                    }

                    var expression = methodCall.Arguments[1];


                    var objectMember = Expression.Convert(expression, typeof(object));
                    var getterLambda = Expression.Lambda<Func<object>>(objectMember).Compile();

                    builder.ExpandParameter = getterLambda().ToString();
                }

                break;
            default:
                return ExecuteMethod(methodCall, builder, resultLoader, intermediateResultLoader);
        }

        return null;
    }

    private static object ResolveProjection(ParameterBuilder builder, LambdaExpression lambdaExpression)
    {

        var selectFunction = lambdaExpression.Body as NewExpression;

        if (selectFunction != null)
        {
            var members = selectFunction.Members.Select(x => x.Name)
                                        .ToArray();
            var args = selectFunction.Arguments.OfType<MemberExpression>()
                                     .Select(x => x.Member.Name)
                                     .ToArray();
            if (members.Intersect(args).Count() != members.Length)
            {
                throw new InvalidOperationException("Projection into new member names is not supported.");
            }

            builder.SelectParameter = string.Join(",", args);
        }

        var propertyExpression = lambdaExpression.Body as MemberExpression;
        if (propertyExpression != null)
        {
            builder.SelectParameter = string.IsNullOrWhiteSpace(builder.SelectParameter)
                ? propertyExpression.Member.Name
                : builder.SelectParameter + "," + propertyExpression.Member.Name;
        }

        return null;
    }

    private static object InvokeEager(MethodCallExpression methodCall, object source)
    {

        var results = source as IEnumerable;


        var parameters = ResolveInvocationParameters(results, methodCall);
        return methodCall.Method.Invoke(null, parameters);
    }

    private static object[] ResolveInvocationParameters(IEnumerable results, MethodCallExpression methodCall)
    {

        var parameters = new object[] { results.AsQueryable() }
            .Concat(methodCall.Arguments.Where((x, i) => i > 0).Select(GetExpressionValue))
            .Where(x => x != null)
            .ToArray();
        return parameters;
    }

    private static object GetExpressionValue(Expression expression)
    {
        if (expression is UnaryExpression unaryExpression)
        {
            return unaryExpression.Operand;
        }

        if (expression is ConstantExpression constantExpression)
        {
            return constantExpression.Value;
        }

        return null;
    }

    private object GetMethodResult<T>(MethodCallExpression methodCall, ParameterBuilder builder, Func<ParameterBuilder, IEnumerable<T>> resultLoader, Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {

        ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);

        var processResult = _writer.Write(methodCall.Arguments[1]);
        var currentParameter = string.IsNullOrWhiteSpace(builder.FilterParameter)
                                ? processResult
                                : string.Format("({0}) and ({1})", builder.FilterParameter, processResult);
        builder.FilterParameter = currentParameter;

        var genericArguments = methodCall.Method.GetGenericArguments();
        var queryableMethods = typeof(Queryable).GetMethods();

        var nonGenericMethod = queryableMethods
            .Single(x => x.Name == methodCall.Method.Name && x.GetParameters().Length == 1);


        var method = nonGenericMethod
            .MakeGenericMethod(genericArguments);

        var list = resultLoader(builder);


        var queryable = list.AsQueryable();
        var parameters = new object[] { queryable };
        var result = method.Invoke(null, parameters);
        return result ?? default(T);
    }

    private object GetResult<T>(MethodCallExpression methodCall, ParameterBuilder builder, Func<ParameterBuilder, IEnumerable<T>> resultLoader, Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {


        ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
        var results = resultLoader(builder);


        var parameters = ResolveInvocationParameters(results, methodCall);
        var final = methodCall.Method.Invoke(null, parameters);
        return final;
    }

    private object ExecuteMethod<T>(MethodCallExpression methodCall, ParameterBuilder builder, Func<ParameterBuilder, IEnumerable<T>> resultLoader, Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {

        var innerMethod = methodCall.Arguments[0] as MethodCallExpression;

        if (innerMethod == null)
        {
            return null;
        }

        var result = ProcessMethodCall(innerMethod, builder, resultLoader, intermediateResultLoader);
        if (result != null)
        {
            return InvokeEager(innerMethod, result);
        }

        var genericArgument = innerMethod.Method.ReturnType.GetGenericArguments()[0];
        var type = typeof(T);
        var list = type != genericArgument
         ? intermediateResultLoader(genericArgument, builder)
         : resultLoader(builder);


        var arguments = ResolveInvocationParameters(list, methodCall);

        return methodCall.Method.Invoke(null, arguments);
    }
}
