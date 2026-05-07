using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

// app connect - u user - p pass run

[TestFixture]
public class SubclassTests
{
    [Test]
    public void Connect_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "connect", "-u", "user", "-p", "pass", "run" };

        var expected = new SubclassExample
        {
            Connect = new() { User = "user", Pass = "pass" },
            Run = true
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SubclassExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

     [Test]
     public void Connect_ShouldFail_WhenSubclassAlsoHasRunProperty()
     {
         // Arrange
         var args = new[] { "connect", "-u", "user", "-p", "pass", "run" };     // conflicts between root and subclass properties but shouldnt conflict between two subclasses

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SubclassWithRunExample>(args);

         // Assert
         errors.Should().NotBeNull();
         position.Should().BeNull();
     }
}