// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig.Attributes;

/// <summary>
/// Indicates that the field or property it is applied to will receive all values that come after <c>--</c> in the arguments.
/// </summary>
/// <remarks>Works with string collection types only.</remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ArgsPathspecAttribute : BaseArgsAttribute;