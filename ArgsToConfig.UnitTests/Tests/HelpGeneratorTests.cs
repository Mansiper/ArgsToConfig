using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class HelpGeneratorTests
{
    [SetUp]
    public void SetUp() => HelpGenerator.ClearCache();

    [Test]
    public void GetHelp_ReturnsNonEmptyString()
    {
        var help = HelpGenerator.GetHelp<GitCommitExample>();
        help.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetHelp_ContainsExpectedDescriptions()
    {
        var help = HelpGenerator.GetHelp<GitCommitExample>();

        help.Should().Contain("Signed-off-by");
        help.Should().Contain("--amend");
        help.Should().Contain("--dry-run");
        help.Should().Contain("-F, --file");
        help.Should().Contain("-m, --message");
        help.Should().Contain("--author");
        help.Should().Contain("--date");
        help.Should().Contain("--cleanup");
        help.Should().Contain("--trailer");
        help.Should().Contain("[--] <pathspec>...");
    }

    [Test]
    public void GetHelp_IsCached_ReturnsSameReference()
    {
        var help1 = HelpGenerator.GetHelp<GitCommitExample>();
        var help2 = HelpGenerator.GetHelp<GitCommitExample>();
        ReferenceEquals(help1, help2).Should().BeTrue();
    }

    [Test]
    public void ClearCache_AllowsRegeneration()
    {
        var help1 = HelpGenerator.GetHelp<GitCommitExample>();
        HelpGenerator.ClearCache();
        var help2 = HelpGenerator.GetHelp<GitCommitExample>();
        help1.Should().Be(help2);
        ReferenceEquals(help1, help2).Should().BeFalse();
    }

    [Test]
    public void GetHelp_EnumPropertyDescriptions_AreIncluded()
    {
        var help = HelpGenerator.GetHelp<GitCommitExample>();
        // CommitMode enum description
        help.Should().Contain("Stage all modifications");
        help.Should().Contain("interactively select");
    }

    [Test]
    public void GetHelp_ValueForBool_ShowsBothFlags()
    {
        var help = HelpGenerator.GetHelp<GitCommitExample>();
        // SignOff shows both -s/--signoff and --no-signoff
        help.Should().Contain("-s, --signoff / --no-signoff");
    }

    [Test]
    public void GetHelp_ByType_ProducesSameResultAsGeneric()
    {
        var byType = HelpGenerator.GetHelp(typeof(GitCommitExample));
        HelpGenerator.ClearCache();
        var byGeneric = HelpGenerator.GetHelp<GitCommitExample>();
        byType.Should().Be(byGeneric);
    }

    [Test]
    public void GetHelp_HelpGroup_SectionHeadersAreEmitted()
    {
        var help = HelpGenerator.GetHelp<HelpGroupExample>();

        help.Should().Contain("Output options:");
        help.Should().Contain("Authentication:");
    }

    [Test]
    public void GetHelp_HelpGroup_UngroupedOptionsAppearBeforeGroups()
    {
        var help = HelpGenerator.GetHelp<HelpGroupExample>();

        var verbosePos = help.IndexOf("--verbose", StringComparison.Ordinal);
        var outputHeaderPos = help.IndexOf("Output options:", StringComparison.Ordinal);

        verbosePos.Should().BeLessThan(outputHeaderPos);
    }

    [Test]
    public void GetHelp_HelpGroup_GroupedOptionsAppearUnderTheirSection()
    {
        var help = HelpGenerator.GetHelp<HelpGroupExample>();

        var outputHeaderPos = help.IndexOf("Output options:", StringComparison.Ordinal);
        var outputOptionPos = help.IndexOf("--output", StringComparison.Ordinal);
        var formatOptionPos = help.IndexOf("--format", StringComparison.Ordinal);

        outputOptionPos.Should().BeGreaterThan(outputHeaderPos);
        formatOptionPos.Should().BeGreaterThan(outputHeaderPos);

        var authHeaderPos = help.IndexOf("Authentication:", StringComparison.Ordinal);
        var userOptionPos = help.IndexOf("--user", StringComparison.Ordinal);
        var passwordOptionPos = help.IndexOf("--password", StringComparison.Ordinal);

        userOptionPos.Should().BeGreaterThan(authHeaderPos);
        passwordOptionPos.Should().BeGreaterThan(authHeaderPos);
    }

    [Test]
    public void GetHelp_HelpGroup_GroupedOptionsDoNotAppearBeforeTheirSection()
    {
        var help = HelpGenerator.GetHelp<HelpGroupExample>();

        var outputHeaderPos = help.IndexOf("Output options:", StringComparison.Ordinal);
        var authHeaderPos = help.IndexOf("Authentication:", StringComparison.Ordinal);

        // Output group comes before Authentication group (declaration order)
        outputHeaderPos.Should().BeLessThan(authHeaderPos);
    }
}