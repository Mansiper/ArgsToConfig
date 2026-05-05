using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

/*
exec pipeline
    pull [--fetch] [--force]
    commit [-m "message"]
    push [--force]
  run [--non-stop]
*/

[TestFixture]
public class PipelineTests
{
    [Test]
    public void Pipeline_ShouldSucceed()
    {
        // Arrange
        var args = new[]
        {
            "pipeline", 
            "pull", "--fetch", 
            "pull", "--force",
            "commit", "-m", "text",
            "push", "--force",
            "run", "--non-stop",
        };

        var expected = new PipelineExample
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

        // Act
        var result = ArgumentsReader.ToObject<PipelineExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Pipeline_ShouldFail_WhenDuplicateCommandNameForSameInterface()
    {
        // Arrange
        var args = new[] { "pull" };

        // Act
        var act = () => ArgumentsReader.ToObject<DuplicatePipelineExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Duplicate*pull*");
    }

    [Test]
    public void Pipeline_ShouldFail_WhenCommandNameMatchesRootParameter()
    {
        // Arrange
        var args = new[] { "run" };

        // Act
        var act = () => ArgumentsReader.ToObject<ConflictingPipelineExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*conflicts with a root parameter*");
    }

    [Test]
    public void Pipeline_ShouldFail_WhenCommandUsesAnotherCommandNameAsArgument()
    {
        // Arrange
        var args = new[] { "pull", "commit" };

        // Act
        var act = () => ArgumentsReader.ToObject<ArgConflictPipelineExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*uses another pipeline command name*");
    }
}
