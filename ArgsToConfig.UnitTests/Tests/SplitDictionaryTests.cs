using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class SplitDictionaryTests
{
    // ── Dictionary<string, string> ─────────────────────────────────────────

    [Test]
    public void Defines_SingleEntry_ShouldSucceed()
    {
        var args = new[] { "--define", "KEY=VALUE" };

        var (result, _, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        result!.Defines.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["KEY"] = "VALUE"
        });
    }

    [Test]
    public void Defines_MultipleEntries_ShouldSucceed()
    {
        var args = new[] { "--define", "A=1", "--define", "B=2", "--define", "C=3" };

        var (result, _, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        result!.Defines.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["A"] = "1",
            ["B"] = "2",
            ["C"] = "3"
        });
    }

    [Test]
    public void Defines_MissingDivider_ShouldFail()
    {
        var args = new[] { "--define", "NOEQUALS" };

        var (_, errors, position) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void Defines_InlineEquals_ShouldSucceed()
    {
        var args = new[] { "--define=KEY=VALUE" };

        var (result, _, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        result!.Defines.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["KEY"] = "VALUE"
        });
    }

    // ── Dictionary<string, int> ────────────────────────────────────────────

    [Test]
    public void Thresholds_MultipleEntries_ShouldSucceed()
    {
        var args = new[] { "--threshold", "cpu=90", "--threshold", "memory=75" };

        var (result, _, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        result!.Thresholds.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["cpu"] = 90,
            ["memory"] = 75
        });
    }

    [Test]
    public void Thresholds_InvalidInt_ShouldFail()
    {
        var args = new[] { "--threshold", "cpu=notanumber" };

        var (_, errors, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        errors.Should().NotBeNull();
    }

    // ── Dictionary<string, (string, int)> ─────────────────────────────────

    [Test]
    public void Entries_SingleTupleValue_ShouldSucceed()
    {
        var args = new[] { "--entry", "item=hello,42" };

        var (result, _, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        result!.Entries.Should().BeEquivalentTo(new Dictionary<string, (string, int)>
        {
            ["item"] = ("hello", 42)
        });
    }

    [Test]
    public void Entries_MultipleEntries_ShouldSucceed()
    {
        var args = new[] { "--entry", "a=foo,1", "--entry", "b=bar,2" };

        var (result, _, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        result!.Entries.Should().BeEquivalentTo(new Dictionary<string, (string, int)>
        {
            ["a"] = ("foo", 1),
            ["b"] = ("bar", 2)
        });
    }

    [Test]
    public void Entries_MissingTupleDivider_ShouldFail()
    {
        var args = new[] { "--entry", "item=hello" }; // missing "," for tuple

        var (_, errors, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        errors.Should().NotBeNull();
    }

    // ── Dictionary<string, string[]> ──────────────────────────────────────

    [Test]
    public void Tags_SingleEntry_ShouldSucceed()
    {
        var args = new[] { "--tags", "env=prod,staging,dev" };

        var (result, _, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        result!.Tags.Should().ContainKey("env");
        result.Tags!["env"].Should().BeEquivalentTo(new[] { "prod", "staging", "dev" });
    }

    [Test]
    public void Tags_MultipleEntries_ShouldSucceed()
    {
        var args = new[] { "--tags", "env=prod,staging", "--tags", "region=us,eu" };

        var (result, _, _) = ArgumentsReader.ToObject<SplitDictionaryExample>(args);

        result!.Tags.Should().ContainKey("env");
        result!.Tags.Should().ContainKey("region");
        result.Tags!["env"].Should().BeEquivalentTo(new[] { "prod", "staging" });
        result.Tags!["region"].Should().BeEquivalentTo(new[] { "us", "eu" });
    }

    // ── ToArgs round-trip ──────────────────────────────────────────────────

    [Test]
    public void Defines_ToArgs_ShouldRoundTrip()
    {
        var config = new SplitDictionaryExample
        {
            Defines = new Dictionary<string, string>
            {
                ["A"] = "1",
                ["B"] = "2"
            }
        };

        var args = ArgumentsReader.ToArgs(config);

        args.Should().Contain("--define");
        args.Should().Contain("A=1");
        args.Should().Contain("B=2");
    }

    [Test]
    public void Thresholds_ToArgs_ShouldRoundTrip()
    {
        var config = new SplitDictionaryExample
        {
            Thresholds = new Dictionary<string, int>
            {
                ["cpu"] = 90
            }
        };

        var args = ArgumentsReader.ToArgs(config);

        args.Should().Contain("--threshold");
        args.Should().Contain("cpu=90");
    }
}