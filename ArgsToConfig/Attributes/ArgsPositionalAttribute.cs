namespace ArgsToConfig.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsPositionalAttribute : Attribute
{
    private int Position { get; }
    public ArgsPositionalAttribute(int position) => 
        Position = position;

    internal int GetPosition => Position;
}