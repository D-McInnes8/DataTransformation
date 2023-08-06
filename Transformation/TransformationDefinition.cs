namespace Transformation;

public class TransformationDefinition
{
    public required bool Enabled { get; init; }
    public required TransformationExpression Expression { get; init; }
    public ICollection<TransformationAction> Actions { get; init; } = Array.Empty<TransformationAction>();

    public override string ToString() => $"IF {Expression} THEN SET {string.Join(", ", Actions)}";
}