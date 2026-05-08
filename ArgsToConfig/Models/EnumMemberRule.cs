namespace ArgsToConfig.Models;

internal sealed class EnumMemberRule
{
    public object Value { get; init; } = null!;
    // ArgsValue on enum member
    public string[]? ArgsValue { get; init; }
}