namespace ArgsToConfig.Models;

internal sealed class EnumMemberRule
{
    public object Value { get; init; } = null!;
    // ArgsHasParameter on enum member
    public string[]? HasParameterNames { get; init; }
    // ArgsValue on enum member
    public string? ArgsValue { get; init; }
}