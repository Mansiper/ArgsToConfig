using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

/*
git clone path
or
git commit [-m <msg>]
 */

[TestFixture]
public class GitMultipleTests
{
    [Test]
    public void Clone_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "clone", "path_to_repo" };

        var expected = new GitMultipleExample
        {
            Clone = new() { Path = "path_to_repo" },
            Commit = null
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<GitMultipleExample>(args);
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Commit_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "commit", "-m", "text" };

        var expected = new GitMultipleExample
        {
            Clone = null,
            Commit = new() { Message = "text" }
        };
     
        // Act
        var (result, _, _) = ArgumentsReader.ToObject<GitMultipleExample>(args);
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Push_ShouldFail_WhenWrongRootArgument()
    {
        // Arrange
        var args = new[] { "push" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<GitMultipleExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(1);
    }
}