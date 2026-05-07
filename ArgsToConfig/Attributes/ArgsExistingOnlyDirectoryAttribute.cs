namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to should only accept existing directory paths when parsing command-line arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsExistingOnlyDirectoryAttribute : Attribute
{
    public string? Description { get; set; }
}