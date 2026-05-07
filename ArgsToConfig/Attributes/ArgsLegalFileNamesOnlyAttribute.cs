namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the argument should only accept legal file names.
/// This attribute can be applied to fields or properties to enforce that the values provided are valid file names, preventing issues related to invalid characters or formats in file paths.
/// The validation logic will automatically determine the operating system and check for the appropriate characters that are not allowed in file names for that system. This helps ensure that the application can handle file paths correctly across different platforms.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsLegalFileNamesOnlyAttribute : Attribute
{
    public string? Description { get; set; }
}