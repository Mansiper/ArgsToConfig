using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class SharedTypeTests
{
    [Test]
    public void SharedEnum_BothSet_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "-a", "yes", "-b", "no" };

        var expected = new SharedEnumExample
        {
            A = SharedEnumYesNo.Yes,
            B = SharedEnumYesNo.No
        };

        // Act
        var result = ArgumentsReader.ToObject<SharedEnumExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void SharedEnum_OnlyOneSet_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "-a", "yes" };

        var expected = new SharedEnumExample
        {
            A = SharedEnumYesNo.Yes,
            B = null
        };

        // Act
        var result = ArgumentsReader.ToObject<SharedEnumExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void SharedClass_BothSubcommands_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "source", "-u", "srcuser", "-p", "srcpass", "target", "-u", "tgtuser", "-p", "tgtpass" };

        var expected = new SharedClassExample
        {
            Source = new() { User = "srcuser", Pass = "srcpass" },
            Target = new() { User = "tgtuser", Pass = "tgtpass" }
        };

        // Act
        var result = ArgumentsReader.ToObject<SharedClassExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void SharedClass_OnlyOne_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "source", "-u", "user", "-p", "pass" };

        var expected = new SharedClassExample
        {
            Source = new() { User = "user", Pass = "pass" },
            Target = null
        };

        // Act
        var result = ArgumentsReader.ToObject<SharedClassExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}