// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that all of the specified fields must have a value at the same time.
/// Must be applied to the class, not to individual properties.
/// Can be applied multiple times to define independent mutual-requirement groups.
/// </summary>
/// <remarks>All listed fields must be nullable.</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ArgsMutuallyRequiredAttribute : BaseArgsAttribute
{
    internal readonly string[] Fields;
    
    public ArgsMutuallyRequiredAttribute(params string[] fields) =>
        Fields = fields;
}