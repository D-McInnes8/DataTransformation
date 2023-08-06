using System.Linq.Expressions;
using Tests.Utility;
using Transformation;

namespace Tests;

public class ActionTests
{
    [Test]
    public void Actions_NoCondition()
    {
        var definition = new TransformationBuilder()
            .ConfigureActions(a =>
            {
                a.AddTransformation(nameof(TestModel.Integer32A), TransformationExpression.Constant(1000));
            })
            .Build();

        var original = new TestModel();
        var originalHash = original.GetHashCode();
        var actual = TransformationParser.TransformObject(original, definition);
        
        Assert.Multiple(() =>
        {
            Assert.That(actual.Integer32A, Is.EqualTo(1000));
            Assert.That(original.GetHashCode(), Is.EqualTo(originalHash));
        });
    }

    [Test]
    public void Actions_SetProperty()
    {
        var original = new TestModel(20,347,345, 
            new DateTime(2023, 7, 1), 
            new DateTime(2023, 7, 1),
            5, string.Empty, Guid.Empty);
        var expected = new TestModel(34,567,345,
            new DateTime(2023, 8, 1),
            new DateTime(2023, 8, 1),
            5, string.Empty, Guid.Empty);
        var originalHash = original.GetHashCode();
        
        var definition = new TransformationBuilder()
            .ConfigureActions(c =>
            {
                c.AddTransformation(nameof(TestModel.Integer32A), TransformationExpression.Constant(expected.Integer32A));
                c.AddTransformation(nameof(TestModel.Integer64), TransformationExpression.Constant(expected.Integer64));
                c.AddTransformation(nameof(TestModel.DateTimeB), TransformationExpression.Constant(expected.DateTimeB));
                c.AddTransformation(nameof(TestModel.DateTimeA), TransformationExpression.Constant(expected.DateTimeA));
            })
            .Build();
        
        var actual = TransformationParser.TransformObject(original, definition);
        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(original.GetHashCode(), Is.EqualTo(originalHash));
        });
    }

    [Test]
    public void Actions_SwapProperties()
    {
        var definition = new TransformationBuilder()
            .ConfigureActions(c =>
            {
                c.AddTransformation(nameof(TestModel.Integer32A), TransformationExpression.Property(nameof(TestModel.Integer32B)));
                c.AddTransformation(nameof(TestModel.Integer32B), TransformationExpression.Property(nameof(TestModel.Integer32A)));
            })
            .Build();

        var original = new TestModel();
        var originalHash = original.GetHashCode();
        var actual = TransformationParser.TransformObject(original, definition);
        
        Assert.Multiple(() =>
        {
            Assert.That(actual.Integer32A, Is.EqualTo(original.Integer32B));
            Assert.That(actual.Integer32B, Is.EqualTo(original.Integer32A));
            Assert.That(original.GetHashCode(), Is.EqualTo(originalHash));
        });
    }

    [Test]
    [TestCase(false, 1000, 500)]
    [TestCase(true, 1000, 250)]
    [Parallelizable(ParallelScope.All)]
    public void Actions_MultipleTransformationsWithSwappedProperties(bool useAccumulateFunc, int expectedInt32A, int expectedInt32B)
    {
        var definitions = new List<TransformationDefinition>
        {
            new TransformationBuilder()
                .ConfigureActions(a =>
                {
                    a.AddTransformation(nameof(TestModel.Integer32A), TransformationExpression.Constant(250));
                })
                .Build(),
            new TransformationBuilder()
                .ConfigureActions(a =>
                {
                    a.AddTransformation(nameof(TestModel.Integer32A), TransformationExpression.Property(nameof(TestModel.Integer32B)));
                    a.AddTransformation(nameof(TestModel.Integer32B), TransformationExpression.Property(nameof(TestModel.Integer32A)));
                })
                .Build()
        };

        var original = new TestModel()
        {
            Integer32A = 500,
            Integer32B = 1000
        };
        var originalHash = original.GetHashCode();
        var actual = useAccumulateFunc 
            ? TransformationParser.TransformObjectAccumulate(original, definitions)
            : TransformationParser.TransformObject(original, definitions);
        
        Assert.Multiple(() =>
        {
            Assert.That(actual.Integer32A, Is.EqualTo(expectedInt32A));
            Assert.That(actual.Integer32B, Is.EqualTo(expectedInt32B));
            Assert.That(original.GetHashCode(), Is.EqualTo(originalHash));
        });
    }
}