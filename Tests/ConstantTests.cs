using System.Linq.Expressions;
using Tests.Utility;
using Transformation;

namespace Tests;

public class ConstantTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    [TestCase(0, 0, true)]
    [TestCase(0, 1, false)]
    [TestCase("String", "String", true)]
    [TestCase("String1", "String2", false)]
    [Parallelizable(ParallelScope.All)]
    public void Constant_Valid_NoActions(object left, object right, bool expected)
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = ExpressionType.Equal;
                r.Left = TransformationExpression.Constant(left);
                r.Right = TransformationExpression.Constant(right);
            })
            .Build();

        var actual = TransformationParser.CompileTransformationRule<int>(definition.Expression)(0);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    [TestCase(0, 0, true)]
    [TestCase(0, "String2", false)]
    [TestCase(false, 0, false)]
    [TestCase(null, null, true)]
    [TestCase(null, "String", true)]
    [Parallelizable(ParallelScope.All)]
    public void Constant_ExpressionCompilation(object left, object right, bool expected)
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = ExpressionType.Equal;
                r.Left = TransformationExpression.Constant(left);
                r.Right = TransformationExpression.Constant(right);
            })
            .Build();

        var actual = TransformationParser.ValidateTransformation<object>(definition);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    [TestCase(nameof(TestModel.Integer32A), 1, 1)]
    [TestCase(nameof(TestModel.Integer64), 1L, 1L)]
    [Parallelizable(ParallelScope.All)]
    public void Constant_ActionTypes_Numbers(string propertyName, object value, object expected)
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = ExpressionType.Equal;
                r.Left = TransformationExpression.Constant(true);
                r.Right = TransformationExpression.Constant(true);
            })
            .ConfigureActions(a =>
            {
                a.AddTransformation(propertyName, TransformationExpression.Constant(value));
            })
            .Build();

        var original = new TestModel();
        var actual = TransformationParser.TransformObject(original, definition);
        
        Assert.That(typeof(TestModel).GetProperty(propertyName)?.GetValue(actual), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("2023-1-10", "2023-1-10")]
    [Parallelizable(ParallelScope.All)]
    public void Constant_ActionTypes_DateTime(DateTime value, DateTime expected)
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = ExpressionType.Equal;
                r.Left = TransformationExpression.Constant(true);
                r.Right = TransformationExpression.Constant(true);
            })
            .ConfigureActions(a =>
            {
                a.AddTransformation(nameof(TestModel.DateTimeA), TransformationExpression.Constant(value));
            })
            .Build();

        var original = new TestModel();
        var actual = TransformationParser.TransformObject(original, definition);
        
        Assert.That(actual.DateTimeA, Is.EqualTo(expected));
    }

    [Test]
    public void Constant_ActionMultiple()
    {
        static TransformationDefinition GenerateDefinition(string propertyName, TransformationExpression expression)
        {
            return new TransformationBuilder()
                .ConfigureRule(r =>
                {
                    r.Operator = ExpressionType.Equal;
                    r.Left = TransformationExpression.Constant(true);
                    r.Right = TransformationExpression.Constant(true);
                })
                .ConfigureActions(a =>
                {
                    a.AddTransformation(propertyName, expression);
                })
                .Build();
        }

        var definitions = new List<TransformationDefinition>()
        {
            GenerateDefinition(nameof(TestModel.Integer32A), TransformationExpression.Constant(0)),
            GenerateDefinition(nameof(TestModel.Integer32B), TransformationExpression.Constant(0)),
            GenerateDefinition(nameof(TestModel.DateTimeA), TransformationExpression.Constant(DateTime.MinValue)),
            GenerateDefinition(nameof(TestModel.DateTimeB), TransformationExpression.Constant(DateTime.MinValue)),
            GenerateDefinition(nameof(TestModel.Integer64), TransformationExpression.Constant(0L)),
            GenerateDefinition(nameof(TestModel.Integer32C), TransformationExpression.Constant(0)),
            GenerateDefinition(nameof(TestModel.Text), TransformationExpression.Constant(string.Empty)),
            GenerateDefinition(nameof(TestModel.Identifier), TransformationExpression.Constant(Guid.Empty))
        };

        var expected = new TestModel
        {
            Integer32A = 0,
            Integer32B = 0,
            DateTimeA = DateTime.MinValue,
            DateTimeB = DateTime.MinValue,
            Integer64 = 0L,
            Integer32C = 0,
            Text = string.Empty,
            Identifier = Guid.Empty
        };
        var actual = definitions
            .Aggregate(new TestModel(), (current, definition) 
                => TransformationParser.TransformObject(current, definition));
        
        Assert.That(actual, Is.EqualTo(expected));
    }
}