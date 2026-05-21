// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the name of the enum member as it appears in the command-line arguments.
/// </summary>
/// <remarks>Works with enum members only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsEnumValueAttribute : BaseArgsAttribute
{
    private readonly string name;

    public ArgsEnumValueAttribute(string name) =>
        this.name = name;

    internal string[] GetValues => name.Split('|');
}