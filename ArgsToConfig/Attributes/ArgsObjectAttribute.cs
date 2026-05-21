// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that starting from the argument with the specified name, the fields of the nested object will be populated.
/// </summary>
/// <remarks>Works with object types only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsObjectAttribute : BaseArgsAttribute
{
    private readonly string name;

    public ArgsObjectAttribute(string name) =>
        this.name = name;

    internal string[] GetNames => name.Split('|');
}