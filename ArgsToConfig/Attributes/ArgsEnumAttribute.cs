namespace ArgsToConfig.Attributes;

/// <summary>
/// Sets the value of the enum to the enum member
/// </summary>
/// <remarks>Works only with enums.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsEnumAttribute : Attribute;