// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ArgsToConfig;

/// <summary>
/// Implement this interface to provide a custom string-to-value convertor for use with <see cref="Attributes.ArgsConvertorAttribute"/>.
/// </summary>
public interface IArgsConvertor
{
    /// <summary>
    /// Converts the raw command-line string <paramref name="value"/> to the target type.
    /// </summary>
    /// <param name="value">The raw string value taken from the command-line arguments.</param>
    /// <returns>The converted value as <see cref="object"/>.</returns>
    object Convert(string value);
}