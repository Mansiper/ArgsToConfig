// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

internal class HelpGroupExample
{
    [ArgsHasParameter("--verbose|-v", Description = "Enable verbose output")]
    public bool Verbose { get; set; }

    [ArgsValueFor("--output|-o", Description = "Output file path")]
    [ArgsHelpGroup("Output options")]
    public string? Output { get; set; }

    [ArgsValueFor("--format", Description = "Output format (json, xml, csv)")]
    [ArgsHelpGroup("Output options")]
    public string? Format { get; set; }

    [ArgsValueFor("--user", Description = "Username for authentication")]
    [ArgsHelpGroup("Authentication")]
    public string? User { get; set; }

    [ArgsValueFor("--password", Description = "Password for authentication")]
    [ArgsHelpGroup("Authentication")]
    public string? Password { get; set; }
}