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

    [Test]
    public void Pipeline_ShouldSucceed_WithListCollection()
    {
        // Arrange
        var args = new[] { "pull", "--fetch", "commit", "-m", "text" };

        // Act
        var result = ArgumentsReader.ToObject<PipelineWithListExample>(args);

        // Assert
        result.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
        {
            new PullCommand { Fetch = true },
            new CommitCommand { Message = "text" }
        }, o => o.RespectingRuntimeTypes());
    }

    [Test]
    public void Pipeline_ShouldSucceed_WithIEnumerableCollection()
    {
        // Arrange
        var args = new[] { "pull", "--fetch", "commit", "-m", "text" };

        // Act
        var result = ArgumentsReader.ToObject<PipelineWithIEnumerableExample>(args);

        // Assert
        result.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
        {
            new PullCommand { Fetch = true },
            new CommitCommand { Message = "text" }
        }, o => o.RespectingRuntimeTypes());
    }

    [Test]
    public void Pipeline_ShouldSucceed_WithICollectionCollection()
    {
        // Arrange
        var args = new[] { "pull", "--fetch", "commit", "-m", "text" };

        // Act
        var result = ArgumentsReader.ToObject<PipelineWithICollectionExample>(args);

        // Assert
        result.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
        {
            new PullCommand { Fetch = true },
            new CommitCommand { Message = "text" }
        }, o => o.RespectingRuntimeTypes());
    }

    [Test]
    public void Pipeline_ShouldSucceed_WithIListCollection()
    {
        // Arrange
        var args = new[] { "pull", "--fetch", "commit", "-m", "text" };

        // Act
        var result = ArgumentsReader.ToObject<PipelineWithIListExample>(args);

        // Assert
        result.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
        {
            new PullCommand { Fetch = true },
            new CommitCommand { Message = "text" }
        }, o => o.RespectingRuntimeTypes());
    }

    [Test]
    public void Pipeline_ShouldSucceed_WithHashSetCollection()
    {
        // Arrange
        var args = new[] { "pull", "--fetch", "commit", "-m", "text" };

        // Act
        var result = ArgumentsReader.ToObject<PipelineWithHashSetExample>(args);

        // Assert
        result.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
        {
            new PullCommand { Fetch = true },
            new CommitCommand { Message = "text" }
        }, o => o.RespectingRuntimeTypes());
    }
}
