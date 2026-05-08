// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to contains a collection of commands.
/// </summary>
/// <remarks>Works with interface collection types only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsPipelineAttribute : Attribute
{
    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }
}