using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class RecordTests
{
    [Test]
    public void Deploy_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "deploy", "--env", "production", "--tag", "v1.0" };

        var expected = new RecordExample
        {
            Deploy = new RecordDeployOptions { Env = "production", Tag = "v1.0" },
            DryRun = null
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<RecordExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Deploy_WithDryRun_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--dry-run", "deploy", "--env", "staging", "--tag", "v2.0" };

        var expected = new RecordExample
        {
            Deploy = new RecordDeployOptions { Env = "staging", Tag = "v2.0" },
            DryRun = true
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<RecordExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}