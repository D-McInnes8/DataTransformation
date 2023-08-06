using System.Linq.Expressions;

namespace Transformation;

public static class TransformationParser
{
    public static T TransformObject<T>(in T originalObject, in TransformationDefinition transformationDefinitionDefinition) where T : class, IClonable<T>
    {
        var shouldTransform = CompileTransformationRule<T>(transformationDefinitionDefinition.Expression)(originalObject);
        if (!shouldTransform)
            return originalObject;

        var newObject = originalObject.DeepCopy();
        foreach (var action in transformationDefinitionDefinition.Actions)
        {
            var actionFunc = CompileTransformationAction<T>(action);
            actionFunc(originalObject, newObject);
        }

        return newObject;
    }

    public static T TransformObjectAccumulate<T>(in T originalObject,
        in IEnumerable<TransformationDefinition> definitions) where T: class, IClonable<T>
    {
        // When evaluating and applying transformations this function will use the results of any previous transformations
        // in the set, i.e. if a transformations references a property that has been modified by an earlier transformation
        // in the set then it will use the new transformed value.
        return definitions
                .Aggregate(originalObject, (current, definition) 
                    => TransformObject(current, definition));
    }

    public static T TransformObject<T>(in T originalModel, in IEnumerable<TransformationDefinition> definitions)
        where T : class, IClonable<T>
    {
        // When evaluating and applying transformations this function will compare it with the original model only,
        // i.e. if if a transformation references a property that has been modified by an earlier transformation in
        // the set then it will use the original value, not the new transformed value.
        var newObject = originalModel.DeepCopy();

        foreach (var definition in definitions)
        {
            var shouldTransform = CompileTransformationRule<T>(definition.Expression)(originalModel);
            if (!shouldTransform)
                continue;
            
            foreach (var action in definition.Actions)
            {
                var actionFunc = CompileTransformationAction<T>(action);
                actionFunc(originalModel, newObject);
            }
        }

        return newObject;
    }

    public static bool ValidateTransformation<T>(in TransformationDefinition transformationDefinition)
    {
        try
        {
            _ = CompileTransformationRule<T>(transformationDefinition.Expression);
            foreach (var action in transformationDefinition.Actions)
            {
                _ = CompileTransformationAction<T>(action);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public static Func<T, bool> CompileTransformationRule<T>(in TransformationExpression transformationExpression)
    {
        var type = typeof(T);
        var param = Expression.Parameter(type, type.Name);
        var expression = GenerateExpression<T>(transformationExpression, param);
        
        //Console.WriteLine((string?)expression.ToString());
        return Expression
            .Lambda<Func<T, bool>>(expression, param)
            .Compile();
    }

    private static Action<T, T> CompileTransformationAction<T>(in TransformationAction transformationAction)
    {
        var type = typeof(T);
        var paramExistingObject = Expression.Parameter(type, type.Name);
        var paramNewObject = Expression.Parameter(type, type.Name);

        var left = Expression.Property(paramNewObject, transformationAction.PropertyName);
        var right = GenerateExpression<T>(transformationAction.Expression, paramExistingObject);
        var expression = Expression.Assign(left, right);
        
        //Console.WriteLine((string?)expression.ToString());
        return Expression
            .Lambda<Action<T, T>>(expression, paramExistingObject, paramNewObject)
            .Compile();
    }

    private static Expression GenerateExpression<T>(in TransformationExpression expression, in ParameterExpression param)
    {
        switch (expression.Type)
        {
            case TransformationExpressionType.Constant:
                return Expression.Constant(expression.Value);
            case TransformationExpressionType.Property:
                return Expression.Property(param, (string)expression.Value);
            case TransformationExpressionType.Expression:
                if (expression.LeftNode is null || expression.RightNode is null)
                    return Expression.Empty();
                
                var type = (ExpressionType)expression.Value;
                var left = GenerateExpression<T>(expression.LeftNode, param);
                var right = GenerateExpression<T>(expression.RightNode, param);
                return Expression.MakeBinary(type, left, right);
            default:
                throw new ArgumentOutOfRangeException(nameof(expression.Type), "Invalid expression type.");
        }
    }
}