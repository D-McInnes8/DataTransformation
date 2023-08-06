using System.Linq.Expressions;
using Tests.Utility;
using Transformation;

namespace Tests;

public class NestedTests
{
    [Test]
    [TestCase(0, 0, ExpressionType.LessThan, 0, 0)]
    [TestCase(0, 100, ExpressionType.LessThanOrEqual, -400, 500)]
    [TestCase(100, 0, ExpressionType.LessThanOrEqual, 100, 0)]
    [TestCase(100, 100, ExpressionType.Equal, -400, 600)]
    [TestCase(200, 100, ExpressionType.GreaterThan, -400, 700)]
    [TestCase(100, 200, ExpressionType.GreaterThan, 100, 200)]
    [Parallelizable(ParallelScope.All)]
    public void Nested_SwapProperties(int int32A, int int32B, ExpressionType type, int expectedInt32A, int expectedInt32B)
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = type;
                r.Left = TransformationExpression.Expression(ExpressionType.Subtract,
                    TransformationExpression.Property(nameof(TestModel.Integer32A)),
                    TransformationExpression.Property(nameof(TestModel.Integer32B)));
                r.Right = TransformationExpression.Constant(0);
            })
            .ConfigureActions(a =>
            {
                a.AddTransformation(nameof(TestModel.Integer32A), TransformationExpression.Expression(
                    ExpressionType.Subtract, 
                    TransformationExpression.Property(nameof(TestModel.Integer32B)),
                    TransformationExpression.Constant(500))
                );
                a.AddTransformation(nameof(TestModel.Integer32B), TransformationExpression.Expression(
                    ExpressionType.Add, 
                    TransformationExpression.Property(nameof(TestModel.Integer32A)),
                    TransformationExpression.Constant(500))
                );
            })
            .Build();

        var original = new TestModel()
        {
            Integer32A = int32A,
            Integer32B = int32B
        };
        var actual = TransformationParser.TransformObject(original, definition);
        
        Assert.Multiple(() =>
        {
            Assert.That(actual.Integer32A, Is.EqualTo(expectedInt32A));
            Assert.That(actual.Integer32B, Is.EqualTo(expectedInt32B));
        });
    }

    [Test]
    [TestCase(ExpressionType.And, false)]
    [TestCase(ExpressionType.Or, true)]
    [Parallelizable(ParallelScope.All)]
    public void Nested_ConditionalOperator(ExpressionType type, bool expected)
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = type;
                r.Left = TransformationExpression.Expression(ExpressionType.GreaterThan, 
                    TransformationExpression.Property(nameof(TestModel.Integer32A)), 
                    TransformationExpression.Constant(1000));
                r.Right = TransformationExpression.Expression(ExpressionType.Equal,
                        TransformationExpression.Property(nameof(TestModel.DateTimeA)),
                        TransformationExpression.Property(nameof(TestModel.DateTimeB)));
            })
            .Build();

        var model = new TestModel
        {
            Integer32A = 1000,
            Integer32B = 2000,
            Integer32C = 3,
            Integer64 = 347665,
            DateTimeA = new DateTime(2023, 1, 1),
            DateTimeB = new DateTime(2023, 1, 1)
        };
        var expression = TransformationParser.CompileTransformationRule<TestModel>(definition.Expression);
        var actual = expression(model);
        
        Assert.That(actual, Is.EqualTo(expected));
    }
    
    [Test]
    [TestCase(ExpressionType.And, ExpressionType.And, false)]
    [TestCase(ExpressionType.And, ExpressionType.Or, true)]
    [TestCase(ExpressionType.Or, ExpressionType.Or, true)]
    [TestCase(ExpressionType.Or, ExpressionType.And, true)]
    [Parallelizable(ParallelScope.All)]
    public void Nested_ConditionalOperatorsMultiple(ExpressionType typeInner, ExpressionType typeOuter, bool expected)
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = typeOuter;
                r.Left = TransformationExpression.Expression(typeInner, 
                    TransformationExpression.Expression(ExpressionType.Equal, 
                        TransformationExpression.Constant(true), 
                        TransformationExpression.Constant(true)), 
                    TransformationExpression.Expression(ExpressionType.Equal,
                        TransformationExpression.Constant(false), 
                        TransformationExpression.Constant(true)));
                r.Right = TransformationExpression.Expression(ExpressionType.Equal,
                    TransformationExpression.Property(nameof(TestModel.DateTimeA)),
                    TransformationExpression.Property(nameof(TestModel.DateTimeB)));
            })
            .Build();

        var model = new TestModel
        {
            Integer32A = 1000,
            Integer32B = 2000,
            Integer32C = 3,
            Integer64 = 347665,
            DateTimeA = new DateTime(2023, 1, 1),
            DateTimeB = new DateTime(2023, 1, 1)
        };
        var expression = TransformationParser.CompileTransformationRule<TestModel>(definition.Expression);
        var actual = expression(model);
        
        Assert.That(actual, Is.EqualTo(expected));
    }
}