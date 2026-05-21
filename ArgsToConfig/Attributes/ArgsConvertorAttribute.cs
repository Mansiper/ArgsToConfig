// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to should be converted using the specified convertor type when parsing command-line arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsConvertorAttribute : BaseArgsAttribute
{
    internal readonly Type ConvertorType;

    public ArgsConvertorAttribute(Type convertorType) => 
        ConvertorType = convertorType;
}