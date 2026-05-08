// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to should be converted using the specified convertor type when parsing command-line arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsConvertorAttribute : Attribute
{
    private readonly Type convertorType;
    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }

    public ArgsConvertorAttribute(Type convertorType) =>
        this.convertorType = convertorType;

    internal Type GetConvertorType => convertorType;
}