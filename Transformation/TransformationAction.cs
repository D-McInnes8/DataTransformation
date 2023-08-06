using System.Linq.Expressions;

namespace Transformation;

public class TransformationAction
{
    public required string PropertyName { get; init; }
    public required TransformationExpression Expression { get; init; }
    
    public override string ToString() => $"{PropertyName} = {Expression}";
}