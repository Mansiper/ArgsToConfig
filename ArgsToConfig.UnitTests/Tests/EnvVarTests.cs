using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class EnvVarTests
{
    private string? tempEnvFile;

    [TearDown]
    public void Cleanup()
    {
        // Remove any env vars set during tests
        Environment.SetEnvironmentVariable("APP_OUTPUT", null);
        Environment.SetEnvironmentVariable("APP_COUNT", null);
        Environment.SetEnvironmentVariable("APP_VERBOSE", null);
        Environment.SetEnvironmentVariable("APP_FORMAT", null);

        // Remove temp .env file if created
        if (tempEnvFile is not null && File.Exists(tempEnvFile))
            File.Delete(tempEnvFile);
        tempEnvFile = null;
    }

    // ── ArgsValueFor ────────────────────────────────────────────────────────

    [Test]
    public void EnvVar_ValueFor_String_UsedWhenArgAbsent()
    {
        Environment.SetEnvironmentVariable("APP_OUTPUT", "/tmp/out");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Output.Should().Be("/tmp/out");
    }

    [Test]
    public void EnvVar_ValueFor_Int_UsedWhenArgAbsent()
    {
        Environment.SetEnvironmentVariable("APP_COUNT", "42");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Count.Should().Be(42);
    }

    [Test]
    public void EnvVar_ValueFor_ArgTakesPrecedenceOverEnvVar()
    {
        Environment.SetEnvironmentVariable("APP_OUTPUT", "/from/env");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>("--output", "/from/arg");
        result!.Output.Should().Be("/from/arg");
    }

    [Test]
    public void EnvVar_ValueFor_AbsentEnvVarLeavesPropertyNull()
    {
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Output.Should().BeNull();
    }

    // ── ArgsHasParameter ────────────────────────────────────────────────────

    [Test]
    public void EnvVar_HasParameter_TrueValueSetsTrue()
    {
        Environment.SetEnvironmentVariable("APP_VERBOSE", "true");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Verbose.Should().BeTrue();
    }

    [Test]
    public void EnvVar_HasParameter_OneValueSetsTrue()
    {
        Environment.SetEnvironmentVariable("APP_VERBOSE", "1");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Verbose.Should().BeTrue();
    }

    [Test]
    public void EnvVar_HasParameter_FalseValueSetsFalse()
    {
        Environment.SetEnvironmentVariable("APP_VERBOSE", "false");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Verbose.Should().BeFalse();
    }

    [Test]
    public void EnvVar_HasParameter_ZeroValueSetsFalse()
    {
        Environment.SetEnvironmentVariable("APP_VERBOSE", "0");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Verbose.Should().BeFalse();
    }

    [Test]
    public void EnvVar_HasParameter_EmptyValueSetsFalse()
    {
        Environment.SetEnvironmentVariable("APP_VERBOSE", "");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Verbose.Should().BeFalse();
    }

    [Test]
    public void EnvVar_HasParameter_ArgFlagTakesPrecedence()
    {
        Environment.SetEnvironmentVariable("APP_VERBOSE", "false");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>("--verbose");
        result!.Verbose.Should().BeTrue();
    }

    // ── ArgsEnum ────────────────────────────────────────────────────────────

    [Test]
    public void EnvVar_Enum_ValidValueParsed()
    {
        Environment.SetEnvironmentVariable("APP_FORMAT", "xml");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>();
        result!.Format.Should().Be(EnvVarFormat.Xml);
    }

    [Test]
    public void EnvVar_Enum_InvalidValueReturnsError()
    {
        Environment.SetEnvironmentVariable("APP_FORMAT", "toml");
        var (_, errors, position) = ArgumentsReader.ToObject<EnvVarExample>();
        errors![0].Should().Contain("toml");
        position.Should().BeNull();
    }

    [Test]
    public void EnvVar_Enum_ArgTakesPrecedenceOverEnvVar()
    {
        Environment.SetEnvironmentVariable("APP_FORMAT", "xml");
        var (result, _, _) = ArgumentsReader.ToObject<EnvVarExample>("--format", "csv");
        result!.Format.Should().Be(EnvVarFormat.Csv);
    }

    // ── .env file ───────────────────────────────────────────────────────────

    [Test]
    public void DotEnv_LoadDotEnv_ParsesKeyValuePairs()
    {
        tempEnvFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.env");
        File.WriteAllText(tempEnvFile, """
            # comment line
            APP_OUTPUT=/tmp/dotenv
            APP_COUNT=7
            APP_VERBOSE=1
            APP_FORMAT=csv
            """);

        var vars = InnerToObject.LoadDotEnv(tempEnvFile);
        vars.Should().ContainKey("APP_OUTPUT").WhoseValue.Should().Be("/tmp/dotenv");
        vars.Should().ContainKey("APP_COUNT").WhoseValue.Should().Be("7");
        vars.Should().ContainKey("APP_VERBOSE").WhoseValue.Should().Be("1");
        vars.Should().ContainKey("APP_FORMAT").WhoseValue.Should().Be("csv");
    }

    [Test]
    public void DotEnv_LoadDotEnv_StripsQuotes()
    {
        tempEnvFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.env");
        File.WriteAllText(tempEnvFile, """
            KEY1="double quoted"
            KEY2='single quoted'
            """);

        var vars = InnerToObject.LoadDotEnv(tempEnvFile);
        vars["KEY1"].Should().Be("double quoted");
        vars["KEY2"].Should().Be("single quoted");
    }

    [Test]
    public void DotEnv_LoadDotEnv_MissingFileReturnsEmpty()
    {
        var vars = InnerToObject.LoadDotEnv("/nonexistent/.env");
        vars.Should().BeEmpty();
    }
}