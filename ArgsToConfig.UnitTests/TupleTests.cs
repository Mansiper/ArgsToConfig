using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class TupleTests
{
    [Test]
    public void DoubleStringChar_ValidInput_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--dsc", "1.5_hello.x" };

        var expected = new TupleExample
        {
            DoubleStringChar = (1.5, "hello", 'x')
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void DoubleStringChar_InlineEquals_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--dsc=0.25_world.Z" };

        var expected = new TupleExample
        {
            DoubleStringChar = (0.25, "world", 'Z')
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void DoubleStringChar_MissingDivider_ShouldFail()
    {
        // Arrange
        var args = new[] { "--dsc", "1.5helloNoDivider" };

        // Act
        Action act = () => ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void BoolIntByte_TrueValue_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--bib", "true:99:255" };

        var expected = new TupleExample
        {
            BoolIntByte = (true, 99, 255)
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void BoolIntByte_FalseValue_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--bib", "false:0:0" };

        var expected = new TupleExample
        {
            BoolIntByte = (false, 0, 0)
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void BoolIntByte_ByteOverflow_ShouldFail()
    {
        // Arrange
        var args = new[] { "--bib", "true:1:256" };

        // Act
        Action act = () => ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void BoolIntByte_InvalidBool_ShouldFail()
    {
        // Arrange
        var args = new[] { "--bib", "yes:1:1" };

        // Act
        Action act = () => ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void IntDoubleCharBool_ValidInput_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--idcb", "7;3.14;A;false" };

        var expected = new TupleExample
        {
            IntDoubleCharBool = (7, 3.14, 'A', false)
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void IntDoubleCharBool_NegativeInt_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--idcb", "\"-42;0.001;z;true\"" };

        var expected = new TupleExample
        {
            IntDoubleCharBool = (-42, 0.001, 'z', true)
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void IntDoubleCharBool_MultiCharForChar_ShouldFail()
    {
        // Arrange
        var args = new[] { "--idcb", "7;3.14;AB;true" };

        // Act
        Action act = () => ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Mix_ValidInput_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--mix", "hi|2|9.9|z|true" };

        var expected = new TupleExample
        {
            Mix = ("hi", 2, 9.9, 'z', true)
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Mix_EmptyString_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--mix", "|200|1.0|Q|false" };

        var expected = new TupleExample
        {
            Mix = ("", 200, 1.0, 'Q', false)
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Mix_MissingDivider_ShouldFail()
    {
        // Arrange
        var args = new[] { "--mix", "hi|2|9.9|z" };

        // Act
        Action act = () => ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void MultipleTuples_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--dsc", "2.0_foo.b", "--bib", "true:10:20" };

        var expected = new TupleExample
        {
            DoubleStringChar = (2.0, "foo", 'b'),
            BoolIntByte = (true, 10, 20)
        };

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void NoTuplesSet_ShouldReturnAllNulls()
    {
        // Arrange
        var args = Array.Empty<string>();

        var expected = new TupleExample();

        // Act
        var result = ArgumentsReader.ToObject<TupleExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}