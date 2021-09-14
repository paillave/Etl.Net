using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave
{
    public class SettersExtractor
    {
        public static IDictionary<string, MemberInfo> GetGetters<TIn, TEntity>(Expression<Func<TIn, TEntity>> setValues)
        {
            var vis = new MyVisitor<TIn, TEntity>();
            vis.Visit(setValues);
            return vis.MemberInfos;
        }

        private class MyVisitor<TIn, TEntity> : ExpressionVisitor
        {
            public IDictionary<string, MemberInfo> MemberInfos { get; set; } = new Dictionary<string, MemberInfo>();
            protected override MemberBinding VisitMemberBinding(MemberBinding node)
            {
                if (node.BindingType == MemberBindingType.Assignment)
                {
                    var memberAssignment = (MemberAssignment)node;
                    switch (memberAssignment.Expression)
                    {
                        case MemberExpression me:
                            MemberInfos[memberAssignment.Member.Name] = me.Member;
                            break;
                    }
                }
                return node;
            }
        }
    }
}