namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the argument should only accept legal file names.
/// This attribute can be applied to fields or properties to enforce that the values provided are valid file names, preventing issues related to invalid characters or formats in file paths.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsLegalFileNamesOnlyAttribute : Attribute
{
    public string? Description { get; set; }
}