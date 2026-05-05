using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class PipelineMultipleTests
{
    [Test]
    public void PipelineMultiple_ShouldSucceed()
    {
        // Arrange
        var args = new[]
        {
            "cp11", "-x",
            "cp12",
            "cp13",
            "cp21", "-a",
            "cp22", "-b",
        };

        var expected = new PipelineMultipleExample
        {
            Commands1 =
            [
                new ComandP11 { X = true },
                new ComandP12(),
                new ComandP13(),
            ],
            Commands2 =
            [
                new ComandP21 { A = true },
                new ComandP22 { B = true },
            ],
        };

        // Act
        var result = ArgumentsReader.ToObject<PipelineMultipleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PipelineMultiple_ShouldSucceed_WhenFirstAndThirdPipelinesFilledSkippingSecond()
    {
        // Arrange
        var args = new[]
        {
            "c3p11", "-x",
            "c3p12",
            "c3p31", "-z",
        };

        var expected = new Pipeline3Example
        {
            Commands1 =
            [
                new Comand3P11 { X = true },
                new Comand3P12(),
            ],
            Commands2 = null,
            Commands3 =
            [
                new Comand3P31 { Z = true },
            ],
        };

        // Act
        var result = ArgumentsReader.ToObject<Pipeline3Example>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PipelineMultiple_ShouldSucceed_WhenAllThreePipelinesFilled_MixedOrder()
    {
        // Arrange
        var args = new[]
        {
            "c3p11", "-x",
            "c3p31", "-z",
            "c3p21",
        };
        
        var expected = new Pipeline3Example
        {
            Commands1 =
            [
                new Comand3P11 { X = true },
            ],
            Commands2 =
            [
                new Comand3P21(),
            ],
            Commands3 =
            [
                new Comand3P31 { Z = true },
            ],
        };

        // Act
        var result = ArgumentsReader.ToObject<Pipeline3Example>(args);
        
        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PipelineMultiple_ShouldFail_WhenDuplicateCommandNameAcrossPipelines()
    {
        // Arrange
        var args = new[] { "cpdup" };

        // Act
        var act = () => ArgumentsReader.ToObject<DuplicateCrossMultiplePipelineExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Duplicate*cpdup*");
    }

    [Test]
    public void PipelineMultiple_ShouldFail_WhenMultiplePipelinesUseSameInterface()
    {
        // Arrange
        var args = new[] { "cp11" };

        // Act
        var act = () => ArgumentsReader.ToObject<SameInterfaceMultiplePipelineExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*same interface*");
    }

    [Test]
    public void PipelineMultiple_ShouldFail_WhenGoingBackToPreviousPipelineAfterStartingLaterOne()
    {
        // Arrange
        // cp11 -> Commands1, cp22 -> Commands2, cp12 -> Commands1 (going back, invalid)
        var args = new[]
        {
            "cp11", "-x",
            "cp22", "-b",
            "cp12",
        };

        // Act
        var act = () => ArgumentsReader.ToObject<MixedOrderMultiplePipelineExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot go back*");
    }
}
