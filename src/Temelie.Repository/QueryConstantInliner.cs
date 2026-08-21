using System.Linq.Expressions;

namespace Temelie.Repository;

/// <summary>
/// Partial-evaluates an expression tree, replacing every sub-expression that does not depend on a
/// query parameter (captured locals/closures, member access on constants, etc.) with a
/// <see cref="ConstantExpression"/>. Used so a composed query translates to SQL literals with no
/// EF parameters, which lets the SQL be embedded after an <c>INSERT INTO ... </c> prefix.
/// </summary>
internal static class QueryConstantInliner
{
    public static Expression Inline(Expression expression)
    {
        var candidates = new Nominator().Nominate(expression);
        return new Evaluator(candidates).Visit(expression)!;
    }

    private sealed class Evaluator(HashSet<Expression> candidates) : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node)
        {
            if (node is null)
            {
                return null;
            }

            if (node.NodeType == ExpressionType.Constant || !candidates.Contains(node))
            {
                return base.Visit(node);
            }

            var value = Expression.Lambda(node).Compile().DynamicInvoke();
            return Expression.Constant(value, node.Type);
        }
    }

    private sealed class Nominator : ExpressionVisitor
    {
        private readonly HashSet<Expression> _candidates = [];
        private bool _dependsOnParameter;

        public HashSet<Expression> Nominate(Expression expression)
        {
            Visit(expression);
            return _candidates;
        }

        public override Expression? Visit(Expression? node)
        {
            if (node is null)
            {
                return null;
            }

            var parentDependsOnParameter = _dependsOnParameter;
            _dependsOnParameter = false;

            base.Visit(node);

            if (!_dependsOnParameter)
            {
                if (node.NodeType == ExpressionType.Parameter)
                {
                    _dependsOnParameter = true;
                }
                else if (node.NodeType != ExpressionType.New &&
                         node.NodeType != ExpressionType.MemberInit)
                {
                    // Never fold the object construction itself to a constant; only its captured
                    // argument/binding values are inlined so the projection stays translatable.
                    _candidates.Add(node);
                }
            }

            _dependsOnParameter |= parentDependsOnParameter;
            return node;
        }
    }
}

