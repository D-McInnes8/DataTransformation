using System.Diagnostics.CodeAnalysis;
using Transformation;

namespace Tests.Utility;

public record TestModel : IClonable<TestModel>
{
    public long Integer64 { get; init; }
    public int Integer32A { get; init; }
    public int Integer32B { get; init; }
    public int Integer32C { get; init; }
    public DateTime DateTimeA { get; init; }
    public DateTime DateTimeB { get; init; }
    public string Text { get; init; }
    public Guid Identifier { get; init; }

    public TestModel()
    {
        var random = new Random();
        Integer64 = random.NextInt64(1, long.MaxValue / 5);
        Integer32C = random.Next(1, 15);
        DateTimeB = DateTime.UtcNow - TimeSpan.FromMilliseconds(random.Next(500, 10000));
        DateTimeA = DateTime.UtcNow - TimeSpan.FromMilliseconds(random.Next(10000, 500000));
        Integer32A = random.Next(500, 2500);
        Integer32B = random.Next(500, 2500);
        Text = $"{Guid.NewGuid():N}";
        Identifier = Guid.NewGuid();
    }

    public TestModel(long integer64, int integer32A, int integer32B, DateTime dateTimeA, 
        DateTime dateTimeB, int integer32C, string text, Guid identifier)
    {
        Integer64 = integer64;
        Integer32A = integer32A;
        Integer32B = integer32B;
        DateTimeA = dateTimeA;
        DateTimeB = dateTimeB;
        Integer32C = integer32C;
        Text = text;
        Identifier = identifier;
    }

    public TestModel ShallowCopy() => (TestModel)MemberwiseClone();

    public TestModel DeepCopy()
    {
        return new TestModel()
        {
            Integer32A = Integer32A,
            Integer32B = Integer32B,
            Integer32C = Integer32C,
            DateTimeB = DateTimeB,
            Integer64 = Integer64,
            DateTimeA = DateTimeA,
            Text = new string(Text),
            Identifier = Identifier
        };
    }
}