namespace ArgsToConfig.Attributes;

/// <summary>
/// True if the arguments has a parameter with the name specified in the attribute, false otherwise.
/// </summary>
/// <remarks>Works with boolean type only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsHasParameterAttribute : Attribute
{
    private string Name { get; }
    private int Position { get; }

    public ArgsHasParameterAttribute(string name, int position = -1) => 
        (Name, Position) = (name, position);

    internal string[] GetNames => Name.Split('|');
    internal int GetPosition => Position;
}