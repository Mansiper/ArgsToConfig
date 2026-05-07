namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to should only accept values from a specified set of string values when parsing command-line arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsAcceptFromAmongAttribute : Attribute
{
    private readonly string[] values;
    public string? Description { get; set; }

    public ArgsAcceptFromAmongAttribute(params string[] values) => 
        this.values = values;

    internal string[] GetValues => values;
}