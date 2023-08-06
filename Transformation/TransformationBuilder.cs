using System.Linq.Expressions;

namespace Transformation;

public class TransformationBuilder
{
    private TransformationExpression? _rule;
    private readonly List<TransformationAction> _actions = new();

    public TransformationBuilder ConfigureRule(Action<TransformationRuleBuilder> configure)
    {
        var ruleBuilder = new TransformationRuleBuilder();
        configure(ruleBuilder);
        _rule = ruleBuilder.Build();
        return this;
    }

    public TransformationBuilder ConfigureActions(Action<TransformationActionBuilder> configure)
    {
        var actionBuilder = new TransformationActionBuilder();
        configure(actionBuilder);
        _actions.AddRange(actionBuilder.Actions);
        return this;
    }
    
    public TransformationDefinition Build()
    {
        _rule ??= new TransformationExpression
        {
            Type = TransformationExpressionType.Expression,
            Value = ExpressionType.Equal,
            LeftNode = TransformationExpression.Constant(true),
            RightNode = TransformationExpression.Constant(true)
        };

        return new TransformationDefinition()
        {
            Enabled = true,
            Expression = _rule,
            Actions = _actions
        };
    }
}

public class TransformationRuleBuilder
{
    public ExpressionType Operator { get; set; }
    public TransformationExpression? Left { get; set; }
    public TransformationExpression? Right { get; set; }

    internal TransformationExpression Build()
    {
        if (Left is null || Right is null)
            throw new InvalidOperationException("Both the LHS and RHS must be configured.");

        return new TransformationExpression
        {
            Type = TransformationExpressionType.Expression,
            Value = Operator,
            LeftNode = Left,
            RightNode = Right
        };
    }
}

public class TransformationActionBuilder
{
    internal ICollection<TransformationAction> Actions { get; init; } = new List<TransformationAction>();
    
    public TransformationActionBuilder AddTransformation(in string propertyName, in TransformationExpression expression)
    {
        Actions.Add(new TransformationAction
        {
            PropertyName = propertyName,
            Expression = expression
        });
        return this;
    }
}