// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Groups one or more properties under a named section in the help output.
/// Properties sharing the same group name are listed together under that section header.
/// Properties without this attribute appear in an implicit ungrouped section at the top.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsHelpGroupAttribute : Attribute
{
    /// <summary>Gets the display name of the group shown as a section header in help output.</summary>
    internal readonly string Name;

    public ArgsHelpGroupAttribute(string name) => 
        Name = name;
}