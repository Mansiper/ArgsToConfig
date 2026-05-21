// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to should only accept values from a specified set of string values when parsing command-line arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsAcceptFromAmongAttribute : BaseArgsAttribute
{
    internal readonly string[] Values;

    public ArgsAcceptFromAmongAttribute(params string[] values) =>
        Values = values;
}