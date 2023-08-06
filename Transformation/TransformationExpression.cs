using System.Linq.Expressions;

namespace Transformation;

public class TransformationExpression
{
    public required TransformationExpressionType Type { get; init; }
    public required object Value { get; init; }
    public TransformationExpression? LeftNode { get; init; }
    public TransformationExpression? RightNode { get; init; }

    private static readonly ISet<ExpressionType> ValidExpressionTypes = new HashSet<ExpressionType>()
    {
        ExpressionType.Equal, ExpressionType.NotEqual, ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual,
        ExpressionType.LessThan, ExpressionType.LessThanOrEqual, ExpressionType.And, ExpressionType.Not, ExpressionType.Or,
        ExpressionType.Add, ExpressionType.Subtract, ExpressionType.Multiply, ExpressionType.Divide, 
        ExpressionType.Modulo,ExpressionType.Negate, ExpressionType.LeftShift, ExpressionType.RightShift, ExpressionType.ExclusiveOr
    };

    public static TransformationExpression Constant<T>(in T value) where T : notnull => new()
    {
        Type = TransformationExpressionType.Constant,
        Value = value
    };

    public static TransformationExpression Property(in string propertyName) => new()
    {
        Type = TransformationExpressionType.Property,
        Value = propertyName
    };

    public static TransformationExpression Expression(in ExpressionType expressionOperator, in TransformationExpression left,
        in TransformationExpression right)
    {
        if (!ValidExpressionTypes.Contains(expressionOperator))
            throw new ArgumentOutOfRangeException(nameof(expressionOperator), "Operator is invalid.");
        
        return new TransformationExpression
        {
            Type = TransformationExpressionType.Expression,
            Value = expressionOperator,
            LeftNode = left,
            RightNode = right
        };
    }
    
    public override string ToString() => Type switch
    {
        TransformationExpressionType.Constant => Value.ToString() ?? string.Empty,
        TransformationExpressionType.Property => Value.ToString() ?? string.Empty,
        TransformationExpressionType.Expression =>  $"({LeftNode} {Value} {RightNode})",
        _ => string.Empty
    };
}