// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to can only be assigned a value if all fields with the names specified in the attribute parameters are not <see langword="null"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsIfSetAttribute : Attribute
{
    internal readonly string[] Fields;

    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }

    public ArgsIfSetAttribute(params string[] fields) =>
        Fields = fields;
}