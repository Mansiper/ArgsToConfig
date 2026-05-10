// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Maps the argument value to an enum member using the values defined by <see cref="ArgsEnumValueAttribute"/> on enum members.
/// Multiple argument names can be specified by separating them with <c>|</c>.
/// </summary>
/// <remarks>Works only with enums.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsEnumAttribute : Attribute
{
    private readonly string? name;

    /// <summary>Gets or sets a value indicating whether the argument is optional. When <see langword="true"/> and the argument is absent, <see cref="DefaultValue"/> is used.</summary>
    public bool Optional { get; set; }
    /// <summary>Gets or sets the default enum member name (as defined by <see cref="ArgsEnumValueAttribute"/>) used when the argument is absent and <see cref="Optional"/> is <see langword="true"/>.</summary>
    public string? DefaultValue { get; set; }
    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the name of an environment variable that supplies the value when the argument is absent.</summary>
    public string? EnvVar { get; set; }

    public ArgsEnumAttribute() { }

    public ArgsEnumAttribute(string name) =>
        this.name = name;

    internal string[]? GetNames => name?.Split('|');
}