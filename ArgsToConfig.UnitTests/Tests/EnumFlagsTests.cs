// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class EnumFlagsTests
{
    // ── ArgsValueOf repeated ─────────────────────────────────────────────────

    [Test]
    public void ValueOf_SingleFlag_ShouldParse()
    {
        // Arrange
        var args = new[] { "--permission", "read" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.Permission.Should().Be(FilePermission.Read);
    }

    [Test]
    public void ValueOf_TwoRepeatedFlags_ShouldCombine()
    {
        // Arrange
        var args = new[] { "--permission", "read", "--permission", "write" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.Permission.Should().Be(FilePermission.Read | FilePermission.Write);
    }

    [Test]
    public void ValueOf_AllThreeFlags_ShouldCombine()
    {
        // Arrange
        var args = new[] { "--permission", "read", "--permission", "write", "--permission", "execute" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.Permission.Should().Be(FilePermission.Read | FilePermission.Write | FilePermission.Execute);
    }

    [Test]
    public void ValueOf_InvalidValue_ShouldFail_AtCorrectPosition()
    {
        // Arrange
        // args: index 0 = "--permission", index 1 = "invalid"
        // error detected at i=1 (value index), position returned = 1+1 = 2
        var args = new[] { "--permission", "invalid" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void ValueOf_InvalidSecondValue_ShouldFail_AtCorrectPosition()
    {
        // Arrange
        // args: 0="--permission" 1="read" 2="--permission" 3="bad"
        // error detected at i=3, position returned = 3+1 = 4
        var args = new[] { "--permission", "read", "--permission", "bad" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(4);
    }

    // ── ArgsSplit dividers ───────────────────────────────────────────────────

    [Test]
    public void Split_TwoValues_ShouldCombine()
    {
        // Arrange
        var args = new[] { "--modes", "read,write" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.Modes.Should().Be(FilePermission.Read | FilePermission.Write);
    }

    [Test]
    public void Split_AllValues_ShouldCombine()
    {
        // Arrange
        var args = new[] { "--modes", "read,write,execute" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.Modes.Should().Be(FilePermission.Read | FilePermission.Write | FilePermission.Execute);
    }

    [Test]
    public void Split_SingleValue_ShouldWork()
    {
        // Arrange
        var args = new[] { "--modes", "execute" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.Modes.Should().Be(FilePermission.Execute);
    }

    [Test]
    public void Split_InvalidValue_ShouldFail_AtCorrectPosition()
    {
        // Arrange
        // args: 0="--modes", 1="read,bad"
        // error detected at i=1, position returned = 1+1 = 2
        var args = new[] { "--modes", "read,bad" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void Split_RepeatedFlagWithSplit_ShouldAccumulateAllValues()
    {
        // Arrange: repeat the flag, each occurrence adds flags via split
        var args = new[] { "--modes", "read,write", "--modes", "execute" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.Modes.Should().Be(FilePermission.Read | FilePermission.Write | FilePermission.Execute);
    }

    // ── Per-member dash flags repeated ───────────────────────────────────────

    [Test]
    public void DashFlags_SingleFlag_ShouldParse()
    {
        // Arrange
        var args = new[] { "--verbose" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.LogFlags.Should().Be(LogFlag.Verbose);
    }

    [Test]
    public void DashFlags_TwoFlags_ShouldCombine()
    {
        // Arrange
        var args = new[] { "--verbose", "--debug" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.LogFlags.Should().Be(LogFlag.Verbose | LogFlag.Debug);
    }

    [Test]
    public void DashFlags_AllFlags_ShouldCombine()
    {
        // Arrange
        var args = new[] { "--verbose", "--debug", "--trace" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.LogFlags.Should().Be(LogFlag.Verbose | LogFlag.Debug | LogFlag.Trace);
    }

    [Test]
    public void DashFlags_ShortNames_ShouldCombine()
    {
        // Arrange
        var args = new[] { "-v", "-d" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.LogFlags.Should().Be(LogFlag.Verbose | LogFlag.Debug);
    }

    [Test]
    public void DashFlags_Absent_ShouldBeNone()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.LogFlags.Should().Be(LogFlag.None);
    }

    // ── Mixed usage ──────────────────────────────────────────────────────────

    [Test]
    public void Mixed_AllThreeProperties_ShouldParseIndependently()
    {
        // Arrange
        var args = new[]
        {
            "--permission", "read", "--permission", "write",
            "--modes", "execute",
            "--verbose", "--trace"
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<EnumFlagsExample>(args);

        // Assert
        result!.Permission.Should().Be(FilePermission.Read | FilePermission.Write);
        result.Modes.Should().Be(FilePermission.Execute);
        result.LogFlags.Should().Be(LogFlag.Verbose | LogFlag.Trace);
    }
}