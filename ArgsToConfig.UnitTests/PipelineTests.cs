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
        var (result, _, _) = ArgumentsReader.ToObject<PipelineExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Pipeline_ShouldFail_WhenDuplicateCommandNameForSameInterface()
    {
        // Arrange
        var args = new[] { "pull" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<DuplicatePipelineExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(1);    //or maybe null?
    }

    [Test]
    public void Pipeline_ShouldFail_WhenCommandNameMatchesRootParameter()
    {
        // Arrange
        var args = new[] { "run" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ConflictingPipelineExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(1);    //or maybe null?
    }

    [Test]
    public void Pipeline_ShouldFail_WhenCommandUsesAnotherCommandNameAsArgument()
    {
        // Arrange
        var args = new[] { "pull", "commit" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<ArgConflictPipelineExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);    //or maybe null?
    }

    [Test]
    public void Pipeline_ShouldSucceed_WithListCollection()
    {
        // Arrange
        var args = new[] { "pull", "--fetch", "commit", "-m", "text" };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<PipelineWithListExample>(args);

        // Assert
        result!.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
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
        var (result, _, _) = ArgumentsReader.ToObject<PipelineWithIEnumerableExample>(args);

        // Assert
        result!.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
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
        var (result, _, _) = ArgumentsReader.ToObject<PipelineWithICollectionExample>(args);

        // Assert
        result!.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
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
        var (result, _, _) = ArgumentsReader.ToObject<PipelineWithIListExample>(args);

        // Assert
        result!.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
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
        var (result, _, _) = ArgumentsReader.ToObject<PipelineWithHashSetExample>(args);

        // Assert
        result!.Commands.Should().BeEquivalentTo(new List<IPipelineCommand>
        {
            new PullCommand { Fetch = true },
            new CommitCommand { Message = "text" }
        }, o => o.RespectingRuntimeTypes());
    }
}