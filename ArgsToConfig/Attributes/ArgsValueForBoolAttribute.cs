namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies two distinct argument values: one that maps to <see langword="true"/> and one that maps to <see langword="false"/>.
/// Multiple argument names can be specified for each value by separating them with <c>|</c>.
/// </summary>
/// <remarks>Works with boolean type only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsValueForBoolAttribute : Attribute
{
    private readonly string trueName;
    private readonly string falseName;
    public ArgsValueForBoolAttribute(string trueName, string falseName) =>
        (this.trueName, this.falseName) = (trueName, falseName);

    internal string[] GetTrueNames => trueName.Split('|');
    internal string[] GetFalseNames => falseName.Split('|');
}