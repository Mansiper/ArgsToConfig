using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class ToArgsTests
{
    // ── Bool flags (ArgsHasParameter) ────────────────────────────────────────

    [Test]
    public void BoolFlag_True_ShouldEmitFlagName()
    {
        var obj = new UnnamedExample { X = true };
        var result = ArgumentsReader.ToArgs(obj);
        result.Should().Contain("-x");
    }

    [Test]
    public void BoolFlag_False_ShouldNotEmitFlagName()
    {
        var obj = new UnnamedExample { X = false };
        var result = ArgumentsReader.ToArgs(obj);
        result.Should().NotContain("-x");
    }

    [Test]
    public void BoolFlag_Null_ShouldNotEmitFlagName()
    {
        var obj = new UnnamedExample { X = null };
        var result = ArgumentsReader.ToArgs(obj);
        result.Should().NotContain("-x");
    }

    // ── Implicit positionals ─────────────────────────────────────────────────

    [Test]
    public void ImplicitPositionals_ShouldAppearInOrder()
    {
        var obj = new UnnamedExample { OldPath = "old", NewPath = "new" };
        var result = ArgumentsReader.ToArgs(obj);
        result.Should().ContainInOrder("old", "new");
    }

    [Test]
    public void ImplicitPositionals_WithFlags_ShouldRoundtrip()
    {
        var obj = new UnnamedExample { OldPath = "src", NewPath = "dst", X = true, Y = true };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<UnnamedExample>(args);
        parsed.Should().BeEquivalentTo(obj);
    }

    // ── ArgsPositional ────────────────────────────────────────────────────────

    [Test]
    public void ExplicitPositionals_ShouldRoundtrip()
    {
        var obj = new UnnamedPosExample { OldPath = "src", NewPath = "dst", Z = true };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<UnnamedPosExample>(args);
        parsed.Should().BeEquivalentTo(obj);
    }

    // ── ArgsValueFor ──────────────────────────────────────────────────────────

    [Test]
    public void ValueFor_ShouldEmitFlagAndValue()
    {
        var obj = new SubclassConnection { User = "alice", Pass = "secret" };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().ContainInOrder("-u", "alice");
        args.Should().ContainInOrder("-p", "secret");
    }

    [Test]
    public void ValueFor_Null_ShouldNotEmit()
    {
        var obj = new GitCommitExample { Message = null };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().NotContain("-m").And.NotContain("--message");
    }

    // ── ArgsValueForBool ──────────────────────────────────────────────────────

    [Test]
    public void ValueForBool_True_ShouldEmitTrueName()
    {
        var obj = new GitCommitExample { SignOff = true };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().Contain("-s");
    }

    [Test]
    public void ValueForBool_False_ShouldEmitFalseName()
    {
        var obj = new GitCommitExample { SignOff = false };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().Contain("--no-signoff");
    }

    [Test]
    public void ValueForBool_Status_ShouldRoundtrip()
    {
        var obj = new GitCommitExample { Status = true };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<GitCommitExample>(args);
        parsed!.Status.Should().BeTrue();
    }

    // ── ArgsEnum ──────────────────────────────────────────────────────────────

    [Test]
    public void Enum_PerMemberHasParameter_ShouldEmitMemberFlag()
    {
        var obj = new GitCommitExample { CommitMode = CommitMode.A };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().Contain("-a");
    }

    [Test]
    public void Enum_WithValueFor_ShouldEmitFlagAndValue()
    {
        var obj = new GitCommitExample { Cleanup = CleanupMode.Strip };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().ContainInOrder("--cleanup", "Strip");
    }

    [Test]
    public void Enum_WithValueFor_DefaultValue_ShouldRoundtrip()
    {
        var obj = new GitCommitExample { Cleanup = CleanupMode.Default };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<GitCommitExample>(args);
        parsed!.Cleanup.Should().Be(CleanupMode.Default);
    }

    // ── ArgsObject (subcommand) ───────────────────────────────────────────────

    [Test]
    public void SubObject_ShouldEmitSubcommandNameAndNestedArgs()
    {
        var obj = new SubclassExample
        {
            Connect = new SubclassConnection { User = "bob", Pass = "pw" },
            Run = true
        };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().Contain("connect");
        args.Should().ContainInOrder("-u", "bob");
        args.Should().ContainInOrder("-p", "pw");
    }

    [Test]
    public void SubObject_ShouldRoundtrip()
    {
        var obj = new SubclassExample
        {
            Connect = new SubclassConnection { User = "alice", Pass = "pass123" },
            Run = true
        };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<SubclassExample>(args);
        parsed.Should().BeEquivalentTo(obj);
    }

    // ── ArgsPipeline ──────────────────────────────────────────────────────────

    [Test]
    public void Pipeline_ShouldEmitCommandNamesAndArgs()
    {
        var obj = new PipelineExample
        {
            Pipeline = true,
            Commands =
            [
                new PullCommand { Fetch = true },
                new CommitCommand { Message = "hello" },
                new PushCommand { Force = true }
            ]
        };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().Contain("pull");
        args.Should().Contain("--fetch");
        args.Should().Contain("commit");
        args.Should().ContainInOrder("-m", "hello");
        args.Should().Contain("push");
        args.Should().Contain("--force");
    }

    [Test]
    public void Pipeline_ShouldRoundtrip()
    {
        var obj = new PipelineExample
        {
            Pipeline = true,
            Commands =
            [
                new PullCommand { Fetch = true },
                new PullCommand { Force = true },
                new CommitCommand { Message = "text" },
                new PushCommand { Force = true }
            ],
            Run = true,
            NonStop = true,
        };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<PipelineExample>(args);
        parsed.Should().BeEquivalentTo(obj);
    }

    // ── ArgsSplit ─────────────────────────────────────────────────────────────

    [Test]
    public void Split_ShouldEmitJoinedValue()
    {
        var obj = new TupleExample { StringInt = ("hello", 42) };
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().ContainInOrder("--sc2", "hello;42");
    }

    [Test]
    public void Split_ShouldRoundtrip()
    {
        var obj = new TupleExample
        {
            StringInt = ("hello", 42),
            ThreeInts = (1, 2, 3),
        };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<TupleExample>(args);
        parsed.Should().BeEquivalentTo(obj);
    }

    // ── Collection (multi-value ArgsValueFor) ────────────────────────────────

    [Test]
    public void Collection_ShouldEmitOnePairPerItem()
    {
        var obj = new GitCommitExample
        {
            Trailer = [("token1", 1), ("token2", 2)]
        };
        var args = ArgumentsReader.ToArgs(obj);
        var trailerIndices = args
            .Select((a, i) => (a, i))
            .Where(x => x.a == "--trailer")
            .Select(x => x.i)
            .ToList();
        trailerIndices.Should().HaveCount(2);
        args[trailerIndices[0] + 1].Should().Be("token1=1");
        args[trailerIndices[1] + 1].Should().Be("token2=2");
    }

    [Test]
    public void Collection_ShouldRoundtrip()
    {
        var obj = new GitCommitExample
        {
            Trailer = [("key", 10), ("other", 99)]
        };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<GitCommitExample>(args);
        parsed!.Trailer.Should().BeEquivalentTo(obj.Trailer);
    }

    // ── Null / default object ────────────────────────────────────────────────

    [Test]
    public void EmptyObject_ShouldProduceNoArgs()
    {
        var obj = new UnnamedExample();
        var args = ArgumentsReader.ToArgs(obj);
        args.Should().BeEmpty();
    }

    // ── Full GitCommit roundtrip ──────────────────────────────────────────────

    [Test]
    public void GitCommit_FullRoundtrip()
    {
        var obj = new GitCommitExample
        {
            IsCommit = true,
            CommitMode = CommitMode.A,
            SignOff = true,
            Verbose = true,
            Amend = true,
            DryRun = true,
            FixupCommit = ("amend", "abc123"),
            File = "file.txt",
            ResetAuthor = true,
            AllowEmpty = true,
            NoVerify = true,
            Edit = true,
            Author = "Jane Doe",
            Date = new DateTime(2024, 1, 15),
            Cleanup = CleanupMode.Strip,
            Status = true,
            IncludeOnly = IncludeOnly.Include,
            PathspecFromFile = "paths.txt",
            PathspecFileNul = true,
            Trailer = [("issue", 42)],
            SignKeyId = "keyid",
        };
        var args = ArgumentsReader.ToArgs(obj);
        var (parsed, _, _) = ArgumentsReader.ToObject<GitCommitExample>(args);
        parsed.Should().BeEquivalentTo(obj);
    }
}