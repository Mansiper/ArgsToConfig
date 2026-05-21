// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the position (zero-based) of a positional argument for the field or property it is applied to.
/// Positional arguments are values without named identifiers. Without this attribute, such fields are assigned values in the order they appear in the class.
/// Use this attribute to define a custom order among positional fields.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsPositionalAttribute : Attribute
{
    internal readonly int Position;

    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }

    public ArgsPositionalAttribute(int position) =>
        Position = position;
}