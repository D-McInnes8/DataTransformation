using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using Transformation;

namespace Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    [Benchmark]
    public void SimpleTransformation()
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
                a.AddTransformation(nameof(BenchmarkModel.Integer32A), TransformationExpression.Constant(0));
            })
            .Build();
        _ = TransformationParser.TransformObject(BenchmarkModel.GenerateRandomBenchmarkModel(), definition);
    }

    [Benchmark]
    public void SwapIfLessThan()
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = ExpressionType.LessThan;
                r.Left = TransformationExpression.Property(nameof(BenchmarkModel.Integer32A));
                r.Right = TransformationExpression.Property(nameof(BenchmarkModel.Integer32B));
            })
            .ConfigureActions(a =>
            {
                a.AddTransformation(nameof(BenchmarkModel.Integer32A),
                    TransformationExpression.Property(nameof(BenchmarkModel.Integer32B)));
                a.AddTransformation(nameof(BenchmarkModel.Integer32B),
                    TransformationExpression.Property(nameof(BenchmarkModel.Integer32A)));
            })
            .Build();
        
        _ = TransformationParser.TransformObject(BenchmarkModel.GenerateRandomBenchmarkModel(), definition);
    }

    [Benchmark]
    public void SwapIfLessThanNested()
    {
        var definition = new TransformationBuilder()
            .ConfigureRule(r =>
            {
                r.Operator = ExpressionType.LessThan;
                r.Left = TransformationExpression.Expression(ExpressionType.Subtract,
                    TransformationExpression.Property(nameof(BenchmarkModel.Integer32A)),
                    TransformationExpression.Property(nameof(BenchmarkModel.Integer32B)));
                r.Right = TransformationExpression.Constant(0);
            })
            .ConfigureActions(a =>
            {
                a.AddTransformation(nameof(BenchmarkModel.Integer32A),
                    TransformationExpression.Property(nameof(BenchmarkModel.Integer32B)));
                a.AddTransformation(nameof(BenchmarkModel.Integer32B),
                    TransformationExpression.Property(nameof(BenchmarkModel.Integer32A)));
            })
            .Build();
        
        _ = TransformationParser.TransformObject(BenchmarkModel.GenerateRandomBenchmarkModel(), definition);
    }

    [Benchmark]
    public void LargeTransformation()
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
            GenerateDefinition(nameof(BenchmarkModel.Integer32A), TransformationExpression.Constant(0)),
            GenerateDefinition(nameof(BenchmarkModel.Integer32B), TransformationExpression.Constant(0)),
            GenerateDefinition(nameof(BenchmarkModel.DateTimeA), TransformationExpression.Constant(DateTime.MinValue)),
            GenerateDefinition(nameof(BenchmarkModel.DateTimeB), TransformationExpression.Constant(DateTime.MinValue)),
            GenerateDefinition(nameof(BenchmarkModel.Integer64), TransformationExpression.Constant(0L)),
            GenerateDefinition(nameof(BenchmarkModel.Text), TransformationExpression.Constant(string.Empty)),
            GenerateDefinition(nameof(BenchmarkModel.Identifier), TransformationExpression.Constant(Guid.Empty))
        };

        var model = BenchmarkModel.GenerateRandomBenchmarkModel();
        _ = definitions
            .Aggregate(model, (current, definition) 
                => TransformationParser.TransformObject(current, definition));
    }
}

public record BenchmarkModel : IClonable<BenchmarkModel>
{
    public long Integer64 { get; set; }
    public int Integer32A { get; set; }
    public int Integer32B { get; set; }
    public DateTime DateTimeA { get; set; }
    public DateTime DateTimeB { get; set; }
    public string? Text { get; set; }
    public Guid Identifier { get; set; }

    public static BenchmarkModel GenerateRandomBenchmarkModel()
    {
        var random = new Random();
        return new BenchmarkModel()
        {
            Integer64 = random.NextInt64(1, long.MaxValue / 5),
            DateTimeB = DateTime.UtcNow - TimeSpan.FromMilliseconds(random.Next(500, 10000)),
            DateTimeA = DateTime.UtcNow - TimeSpan.FromMilliseconds(random.Next(10000, 500000)),
            Integer32A = random.Next(500, 2500),
            Integer32B = random.Next(500, 2500),
            Text = $"{Guid.NewGuid():N}",
            Identifier = Guid.NewGuid()
        };
    }

    public BenchmarkModel ShallowCopy() => (BenchmarkModel)MemberwiseClone();
    public BenchmarkModel DeepCopy() => ShallowCopy();
}