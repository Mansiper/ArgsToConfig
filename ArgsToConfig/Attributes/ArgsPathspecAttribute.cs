namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to will receive all values that come after <c>--</c> in the arguments.
/// </summary>
/// <remarks>Works with string collection types only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsPathspecAttribute : Attribute
{
    public string? Description { get; set; }
}