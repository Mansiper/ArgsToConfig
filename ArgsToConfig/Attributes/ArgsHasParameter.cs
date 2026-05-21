// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Assigns <see langword="true"/> to the field or property if the argument with the specified name was provided; otherwise, the default value is kept.
/// Multiple argument names can be specified by separating them with <c>|</c>.
/// </summary>
/// <remarks>Works with boolean type only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsHasParameterAttribute : Attribute
{
    private readonly string name;
    internal readonly int Position;

    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the name of an environment variable that supplies the value when the argument is absent.</summary>
    public string? EnvVar { get; set; }

    public ArgsHasParameterAttribute(string name, int position = -1) =>
        (this.name, Position) = (name, position);

    internal string[] GetNames => name.Split('|');
}