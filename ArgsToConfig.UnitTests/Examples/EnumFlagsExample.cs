// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// Repeating ArgsValueOf:
//   myapp --permission read --permission write --permission execute
//   => Permission = FilePermission.Read | FilePermission.Write | FilePermission.Execute

// ArgsSplit dividers:
//   myapp --modes "read,write"
//   => Modes = FilePermission.Read | FilePermission.Write

// Per-member dash flags repeated:
//   myapp --read --write
//   => DashFlags = FilePermission.Read | FilePermission.Write

[Flags]
internal enum FilePermission
{
    None    = 0,
    [ArgsEnumValue("read")]    Read    = 1,
    [ArgsEnumValue("write")]   Write   = 2,
    [ArgsEnumValue("execute")] Execute = 4,
}

[Flags]
internal enum LogFlag
{
    None    = 0,
    [ArgsEnumValue("--verbose|-v")] Verbose = 1,
    [ArgsEnumValue("--debug|-d")]   Debug   = 2,
    [ArgsEnumValue("--trace|-t")]   Trace   = 4,
}

internal class EnumFlagsExample
{
    // ArgsValueOf repeated: --permission read --permission write
    [ArgsEnum("--permission", Flags = true, Optional = true)]
    public FilePermission Permission { get; set; }

    // ArgsSplit dividers: --modes "read,write"
    [ArgsEnum("--modes", Flags = true, Optional = true)]
    [ArgsSplit(",")]
    public FilePermission Modes { get; set; }

    // Per-member dash-flags repeated: --verbose --debug
    [ArgsEnum(Flags = true, Optional = true)]
    public LogFlag LogFlags { get; set; }
}
