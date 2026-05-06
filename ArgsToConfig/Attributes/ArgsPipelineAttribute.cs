namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to contains a collection of commands.
/// </summary>
/// <remarks>Works with interface collection types only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsPipelineAttribute : Attribute
{
    public string? Description { get; set; }
}