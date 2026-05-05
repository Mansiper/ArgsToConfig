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
        var result = ArgumentsReader.ToObject<SubclassExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

     [Test]
     public void Connect_ShouldFail_WhenSubclassAlsoHasRunProperty()
     {
         // Arrange
         var args = new[] { "connect", "-u", "user", "-p", "pass", "run" };     // conflicts between root and subclass properties but shouldnt conflict between two subclasses

        // Act
        Action act = () => ArgumentsReader.ToObject<SubclassWithRunExample>(args);
         
         // Assert
         act.Should().Throw<Exception>();
     }
}