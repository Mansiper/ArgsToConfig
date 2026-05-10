namespace ArgsToConfig.Models;

internal sealed class EnumMemberRule
{
    public object Value { get; init; } = null!;
    // ArgsEnumValue on enum member
    public string[]? ArgsEnumValue { get; init; }
}