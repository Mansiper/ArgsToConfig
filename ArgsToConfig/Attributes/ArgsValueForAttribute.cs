// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Specifies the name of a name-value argument. The value of that argument will be assigned to the field or property it is applied to.
/// Multiple argument names can be specified by separating them with <c>|</c>.
/// Works with any primitive type. Also works with tuple types when used together with <see cref="ArgsSplitAttribute"/>.
/// If <see cref="Optional"/> is <see langword="true"/> and the argument is not present, the default value (not <see langword="null"/>) is used instead.
/// The default value can be overridden via <see cref="DefaultValue"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsValueForAttribute : BaseArgsAttribute
{
    private readonly string name;

    /// <summary>Gets or sets a value indicating whether the argument is optional. When <see langword="true"/> and the argument is absent, <see cref="DefaultValue"/> is used.</summary>
    public bool Optional { get; set; }
    /// <summary>Gets or sets the default value used when the argument is absent and <see cref="Optional"/> is <see langword="true"/>.</summary>
    public string? DefaultValue { get; set; }
    /// <summary>Gets or sets the name of an environment variable that supplies the value when the argument is absent.</summary>
    public string? EnvVar { get; set; }

    public ArgsValueForAttribute(string name) =>
        this.name = name;

    internal string[] GetNames => name.Split('|');
}