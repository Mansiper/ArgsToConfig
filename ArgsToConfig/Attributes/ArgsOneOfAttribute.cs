// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that only one of the specified fields can have a value at a time.
/// Must be applied to the class, not to individual properties.
/// Can be applied multiple times to define independent mutual-exclusion groups.
/// </summary>
/// <remarks>All listed fields must be nullable.</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ArgsOneOfAttribute : Attribute
{
    internal readonly string[] Fields;
    
    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }

    public ArgsOneOfAttribute(params string[] fields) =>
        Fields = fields;
}