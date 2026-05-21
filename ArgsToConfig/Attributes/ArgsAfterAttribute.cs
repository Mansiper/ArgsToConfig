// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to must be assigned a value only after all the fields specified in the attribute parameters have been assigned.
/// If the user attempts to modify any of the specified fields after the attributed field or property has been assigned a value, an error will be returned.
/// The specified fields cannot be changed after the value is assigned to the field or property with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsAfterAttribute : BaseArgsAttribute
{
    internal readonly string[] Fields;

    public ArgsAfterAttribute(params string[] fields) =>
        Fields = fields;
}