using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.SqlServer.Core
{
    public class NewInstanceVisitor : ExpressionVisitor
    {
        public List<SqlResultFieldDefinition> MappingSetters { get; private set; } = new List<SqlResultFieldDefinition>();

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Members != null)
                this.MappingSetters.AddRange(node.Members.Zip(node.Arguments, (m, a) => GetMappingSetterDefinition(a, m as PropertyInfo)).ToList());
            return base.VisitNew(node);
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var vis = new UnaryMapping();
            vis.Visit(node.Expression);
            if (vis.ColumnName != null)
            {
                var propertyInfo = node.Member as PropertyInfo;
                MappingSetters.Add(new SqlResultFieldDefinition
                {
                    ColumnName = propertyInfo.Name,
                    PropertyInfo = propertyInfo
                });
            }
            else
            {
                var vis2 = new MappingSetterVisitor();
                vis2.Visit(node.Expression);
                MappingSetters.Add(new SqlResultFieldDefinition
                {
                    ColumnName = vis2.ColumnName,
                    PropertyInfo = node.Member as PropertyInfo
                });
            }
            return base.VisitMemberAssignment(node);
        }
        private SqlResultFieldDefinition GetMappingSetterDefinition(Expression argument, PropertyInfo propertyInfo)
        {
            MappingSetterVisitor vis = new MappingSetterVisitor();
            vis.Visit(argument);
            SqlResultFieldDefinition mappingSetter = new SqlResultFieldDefinition
            {
                ColumnName = vis.ColumnName,
                PropertyInfo = propertyInfo
            };
            return mappingSetter;
        }
        private class UnaryMapping : ExpressionVisitor
        {
            public string ColumnName = null;
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var vis = new MappingSetterVisitor();
                vis.Visit(node.Operand);
                this.ColumnName = vis.ColumnName;
                return null;
            }
        }
    }
}
