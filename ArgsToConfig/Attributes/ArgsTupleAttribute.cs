namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the argument value will be split by the specified dividers and each part will be assigned to the corresponding element of the tuple.
/// </summary>
/// <remarks>
/// Works with tuple types only (e.g. <c>(string, int)</c>).
/// <para>
/// When <see cref="PartsDividers"/> is <see langword="false"/> (default), all parts are separated
/// by the same set of dividers, which are applied cyclically. You only need to provide one divider
/// (or a short repeating pattern) — for example <c>[ArgsTuple(";")]</c> splits a 4-element tuple
/// using <c>;</c> three times.
/// </para>
/// <para>
/// When <see cref="PartsDividers"/> is <see langword="true"/>, each consecutive divider separates
/// the next pair of parts. You must supply exactly <c>N – 1</c> dividers for an <c>N</c>-element
/// tuple, and each divider may be different — for example <c>[ArgsTuple("_", ".", PartsDividers = true)]</c>
/// splits <c>"1.5_hello.x"</c> into <c>(1.5, "hello", 'x')</c>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsTupleAttribute : Attribute
{
    private readonly string[] dividers;

    public string? Description { get; set; }

    /// <summary>
    /// When <see langword="false"/> (default), all parts are separated by the same dividers applied
    /// cyclically. When <see langword="true"/>, each divider separates a specific pair of consecutive
    /// parts (divider <c>i</c> splits between part <c>i</c> and part <c>i+1</c>).
    /// </summary>
    public bool PartsDividers { get; set; }

    public ArgsTupleAttribute(params string[] dividers) =>
        this.dividers = dividers;

    internal string[] GetDividers => dividers;
}