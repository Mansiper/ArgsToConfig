// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to should only accept values from a specified set of string values when parsing command-line arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsAcceptFromAmongAttribute : Attribute
{
    private readonly string[] values;
    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }

    public ArgsAcceptFromAmongAttribute(params string[] values) =>
        this.values = values;

    internal string[] GetValues => values;
}