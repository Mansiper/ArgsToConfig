// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the argument name from which the command starts in the command line for a pipeline command collection.
/// Applied to a class that implements the interface of a command collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ArgsPipelineCommandAttribute : Attribute
{
    internal readonly string Name;

    /// <summary>Gets or sets an optional human-readable description shown in help output.</summary>
    public string? Description { get; set; }

    public ArgsPipelineCommandAttribute(string name) =>
        Name = name;
}