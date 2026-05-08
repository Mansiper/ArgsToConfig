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
}
