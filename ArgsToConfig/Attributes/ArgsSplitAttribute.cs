// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the argument value will be split by the specified dividers and each part will be assigned to the corresponding element of the tuple, dictionary entry, or collection of tuples.
/// </summary>
/// <remarks>
/// Works with:
/// <list type="bullet">
///   <item><description><b>Tuple types</b> (e.g. <c>(string, int)</c>) — splits the value into tuple elements.</description></item>
///   <item><description><b><c>Dictionary&lt;TKey, TValue&gt;</c></b> — the <b>first</b> divider separates the key from the value; remaining dividers split the value further (as a tuple or collection). Repeated flags add entries.</description></item>
///   <item><description><b>Collections of tuples</b> (e.g. <c>(int, int)[]</c>) — each repeated flag occurrence is split into one tuple and appended to the collection.</description></item>
/// </list>
/// <para>
/// When <see cref="PartsDividers"/> is <see langword="false"/> (default), all parts are separated
/// by the same set of dividers, which are applied cyclically. You only need to provide one divider
/// (or a short repeating pattern) — for example <c>[ArgsSplit(";")]</c> splits a 4-element tuple
/// using <c>;</c> three times.
/// </para>
/// <para>
/// When <see cref="PartsDividers"/> is <see langword="true"/>, each consecutive divider separates
/// the next pair of parts. You must supply exactly <c>N – 1</c> dividers for an <c>N</c>-element
/// tuple, and each divider may be different — for example <c>[ArgsSplit("_", ".", PartsDividers = true)]</c>
/// splits <c>"1.5_hello.x"</c> into <c>(1.5, "hello", 'x')</c>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsSplitAttribute : BaseArgsAttribute
{
    internal readonly string[] Dividers;

    /// <summary>
    /// When <see langword="false"/> (default), all parts are separated by the same dividers applied
    /// cyclically. When <see langword="true"/>, each divider separates a specific pair of consecutive
    /// parts (divider <c>i</c> splits between part <c>i</c> and part <c>i+1</c>).
    /// </summary>
    public bool PartsDividers { get; set; }

    public ArgsSplitAttribute(params string[] dividers) =>
        Dividers = dividers;
}