// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the name of the enum member as it appears in the command-line arguments.
/// </summary>
/// <remarks>Works with enum members only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsEnumValueAttribute : Attribute
{
    private readonly string name;

    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }

    public ArgsEnumValueAttribute(string name) =>
        this.name = name;

    internal string[] GetValues => name.Split('|');
}