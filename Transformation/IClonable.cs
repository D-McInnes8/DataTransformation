namespace Transformation;

public interface IClonable<out TSelf> where TSelf : class
{
    public TSelf ShallowCopy();
    public TSelf DeepCopy();
}