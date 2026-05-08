using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class ToArgsStringTests
{
    // ── ArgsHasParameter (bool flag) ─────────────────────────────────────────

    [Test]
    public void BoolFlag_SingleName_ShouldBeWrappedInBrackets()
    {
        var result = ArgumentsReader.ToArgsString<UnnamedExample>();
        result.Should().Contain("[-x]");
        result.Should().Contain("[-y]");
        result.Should().Contain("[-z]");
    }

    [Test]
    public void BoolFlag_MultipleNames_ShouldShowAllNamesWithPipes()
    {
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("[-v | --verbose]");
    }

    [Test]
    public void BoolFlag_PositionalName_ShouldBeWrappedInBrackets()
    {
        // GitCommitExample has [ArgsHasParameter("commit", 0)] — positional name (no dash)
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("[commit]");
    }

    // ── Implicit positionals ─────────────────────────────────────────────────

    [Test]
    public void ImplicitPositionals_ShouldAppearAsAngleBracketed()
    {
        var result = ArgumentsReader.ToArgsString<UnnamedExample>();
        result.Should().Contain("<oldpath>");
        result.Should().Contain("<newpath>");
    }

    // ── ArgsPositional ────────────────────────────────────────────────────────

    [Test]
    public void ExplicitPositionals_ShouldAppearAsAngleBracketed()
    {
        var result = ArgumentsReader.ToArgsString<UnnamedPosExample>();
        result.Should().Contain("<oldpath>");
        result.Should().Contain("<newpath>");
    }

    // ── ArgsValueFor ──────────────────────────────────────────────────────────

    [Test]
    public void ValueFor_Required_ShouldBeAngleBracketed()
    {
        // SubclassConnection: [ArgsValueFor("-u")] with optional=false
        var result = ArgumentsReader.ToArgsString<SubclassConnection>();
        result.Should().Contain("<-u <user>>");
        result.Should().Contain("<-p <pass>>");
    }

    [Test]
    public void ValueFor_Optional_ShouldBeSquareBracketed()
    {
        // GitCommitExample: [ArgsValueFor("-F|--file", true)]
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("[-F <file>]");
        result.Should().Contain("[-m <message>]");
    }

    [Test]
    public void ValueFor_Required_NotWrappedInSquareBrackets()
    {
        var result = ArgumentsReader.ToArgsString<SubclassConnection>();
        result.Should().NotContain("[-u");
        result.Should().NotContain("[-p");
    }

    // ── ArgsValueForBool ──────────────────────────────────────────────────────

    [Test]
    public void ValueForBool_ShouldShowTrueAndFalseNamesWithPipe()
    {
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("[-s | --no-signoff]");
        result.Should().Contain("[--status | --no-status]");
    }

    // ── ArgsEnum (per-member HasParameter) ───────────────────────────────────

    [Test]
    public void Enum_PerMemberHasParameter_ShouldShowAllMembersWithPipes()
    {
        // CommitMode: -a|--all, --interactive, -p|--patch
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("[-a | --interactive | -p]");
    }

    [Test]
    public void Enum_WithValueFor_Optional_ShouldBeSquareBracketed()
    {
        // UntrackedFiles enum backed by ArgsEnum("-u|--untracked-files", true)
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("[-u (normal | all | no)]");
    }

    [Test]
    public void Enum_WithValueFor_Required_ShouldBeAngleBracketed()
    {
        // CleanupMode backed by ArgsEnum("--cleanup") with optional=false
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("<--cleanup (Default | Strip | Whitespace");
    }

    // ── ArgsObject ────────────────────────────────────────────────────────────

    [Test]
    public void ArgsObject_ShouldEmitRootNameAndSubProperties()
    {
        var result = ArgumentsReader.ToArgsString<SubclassExample>();
        result.Should().Contain("connect");
        result.Should().Contain("-u <user>");
        result.Should().Contain("-p <pass>");
    }

    [Test]
    public void ArgsObject_ShouldIncludeOuterFlags()
    {
        var result = ArgumentsReader.ToArgsString<SubclassExample>();
        result.Should().Contain("[run]");
    }

    // ── ArgsPipeline ──────────────────────────────────────────────────────────

    [Test]
    public void Pipeline_ShouldShowPropertyNameWithEllipsis()
    {
        var result = ArgumentsReader.ToArgsString<PipelineExample>();
        result.Should().Contain("[<commands>...]");
    }

    [Test]
    public void Pipeline_ShouldIncludeSurroundingFlags()
    {
        var result = ArgumentsReader.ToArgsString<PipelineExample>();
        result.Should().Contain("[pipeline]");
        result.Should().Contain("[run]");
        result.Should().Contain("[--non-stop]");
    }

    // ── ArgsPathspec ──────────────────────────────────────────────────────────

    [Test]
    public void Pathspec_ShouldShowDoubleDashAndPropertyName()
    {
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("[-- <pathspec>...]");
    }

    // ── Full synopsis shape ───────────────────────────────────────────────────

    [Test]
    public void GitCommitExample_SynopsisContainsExpectedSegmentsInOrder()
    {
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();

        // Major landmarks that should appear left-to-right
        var commitPos = result.IndexOf("[commit]", StringComparison.Ordinal);
        var commitModePos = result.IndexOf("[-a |", StringComparison.Ordinal);
        var signoffPos = result.IndexOf("[-s |", StringComparison.Ordinal);
        var pathspecPos = result.IndexOf("[-- <pathspec>...]", StringComparison.Ordinal);

        commitPos.Should().BeGreaterThanOrEqualTo(0);
        commitModePos.Should().BeGreaterThan(commitPos);
        signoffPos.Should().BeGreaterThan(commitModePos);
        pathspecPos.Should().BeGreaterThan(signoffPos);
    }

    [Test]
    public void UnnamedExample_SynopsisContainsAllFlagsAndPositionals()
    {
        var result = ArgumentsReader.ToArgsString<UnnamedExample>();
        result.Should().Contain("<oldpath>");
        result.Should().Contain("<newpath>");
        result.Should().Contain("[-x]");
        result.Should().Contain("[-y]");
        result.Should().Contain("[-z]");
    }

    // ── ArgsTuple ─────────────────────────────────────────────────────────────

    [Test]
    public void Tuple_SingleDivider_Required_ShouldShowComponentTypesWithDivider()
    {
        // FixupCommit: (string, string) with [ArgsTuple(":")]  — required (optional=false)
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("<--fixup <string>:<string>>");
    }

    [Test]
    public void Tuple_MultiDivider_Collection_Optional_ShouldShowComponentTypesWithDividersAndEllipsis()
    {
        // Trailer: List<(string, int)> with [ArgsTuple("=", ":")] — required (no optional=true)
        var result = ArgumentsReader.ToArgsString<GitCommitExample>();
        result.Should().Contain("<--trailer <string>=|:<int32>...>");
    }
}
