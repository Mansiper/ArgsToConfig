using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests.Tests;

[TestFixture]
public class SplitTests
{
    [Test]
    public void DoubleStringChar_ValidInput_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--dsc", "1.5_hello.x" };

        var expected = new SplitExample
        {
            DoubleStringChar = (1.5, "hello", 'x')
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void DoubleStringChar_InlineEquals_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--dsc=0.25_world.Z" };

        var expected = new SplitExample
        {
            DoubleStringChar = (0.25, "world", 'Z')
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void DoubleStringChar_MissingDivider_ShouldFail()
    {
        // Arrange
        var args = new[] { "--dsc", "1.5helloNoDivider" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void BoolIntByte_TrueValue_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--bib", "true:99:255" };

        var expected = new SplitExample
        {
            BoolIntByte = (true, 99, 255)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void BoolIntByte_FalseValue_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--bib", "false:0:0" };

        var expected = new SplitExample
        {
            BoolIntByte = (false, 0, 0)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void BoolIntByte_ByteOverflow_ShouldFail()
    {
        // Arrange
        var args = new[] { "--bib", "true:1:256" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void BoolIntByte_InvalidBool_ShouldFail()
    {
        // Arrange
        var args = new[] { "--bib", "yes:1:1" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void IntDoubleCharBool_ValidInput_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--idcb", "7;3.14;A;false" };

        var expected = new SplitExample
        {
            IntDoubleCharBool = (7, 3.14, 'A', false)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void IntDoubleCharBool_NegativeInt_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--idcb", "\"-42;0.001;z;true\"" };

        var expected = new SplitExample
        {
            IntDoubleCharBool = (-42, 0.001, 'z', true)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void IntDoubleCharBool_MultiCharForChar_ShouldFail()
    {
        // Arrange
        var args = new[] { "--idcb", "7;3.14;AB;true" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void Mix_ValidInput_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--mix", "hi|2|9.9|z|true" };

        var expected = new SplitExample
        {
            Mix = ("hi", 2, 9.9, 'z', true)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Mix_EmptyString_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--mix", "|200|1.0|Q|false" };

        var expected = new SplitExample
        {
            Mix = ("", 200, 1.0, 'Q', false)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Mix_MissingDivider_ShouldFail()
    {
        // Arrange
        var args = new[] { "--mix", "hi|2|9.9|z" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void MultipleTuples_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--dsc", "2.0_foo.b", "--bib", "true:10:20" };

        var expected = new SplitExample
        {
            DoubleStringChar = (2.0, "foo", 'b'),
            BoolIntByte = (true, 10, 20)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void NoTuplesSet_ShouldReturnAllNulls()
    {
        // Arrange
        var args = Array.Empty<string>();

        var expected = new SplitExample();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    // ── PartsDividers = false (cyclic) ────────────────────────────────────────

    [Test]
    public void StringInt_SingleDivider_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--sc2", "hello;42" };

        var expected = new SplitExample
        {
            StringInt = ("hello", 42)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ThreeInts_SingleDividerCyclic_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--i3", "1,2,3" };

        var expected = new SplitExample
        {
            ThreeInts = (1, 2, 3)
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ThreeInts_MissingDivider_ShouldFail()
    {
        // Arrange
        var args = new[] { "--i3", "1,2" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void AltDividers_CyclicPattern_ShouldSucceed()
    {
        // Arrange – dividers "-" and ":" repeat: "a-1:b" splits as ["a", "1", "b"]
        var args = new[] { "--alt", "a-1:b" };

        var expected = new SplitExample
        {
            AltDividers = ("a", 1, "b")
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void AltDividers_MissingSecondDivider_ShouldFail()
    {
        // Arrange – second divider ":" is missing
        var args = new[] { "--alt", "a-1b" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    // ── Collection of tuples ────────────────────────────────────────────────

    [Test]
    public void Points_MultipleValues_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--points", "1,2", "--points", "3,4", "--points", "5,6" };

        var expected = new SplitExample
        {
            Points = [(1, 2), (3, 4), (5, 6)]
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Points_SingleValue_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--points", "10,20" };

        var expected = new SplitExample
        {
            Points = [(10, 20)]
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Points_MissingDivider_ShouldFail()
    {
        // Arrange
        var args = new[] { "--points", "1020" }; // missing ","

        // Act
        var (_, errors, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
    }

    // ── int[] with ArgsSplit PartsDividers = false ────────────────────────────

    [Test]
    public void NumsFalse_MultipleOccurrences_ShouldCollect()
    {
        // Arrange – single value "10,20,30" split by cyclic divider "," into int[] { 10, 20, 30 }
        var args = new[] { "--nums-false", "10,20,30" };

        var expected = new SplitExample
        {
            NumsFalse = [10, 20, 30]
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void NumsFalse_SingleElement_ShouldSucceed()
    {
        // Arrange – no divider present; single element
        var args = new[] { "--nums-false", "42" };

        var expected = new SplitExample
        {
            NumsFalse = [42]
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void NumsFalse_InvalidInt_ShouldFail()
    {
        // Arrange
        var args = new[] { "--nums-false", "notanint" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void NumsFalse_NotPresent_ShouldBeNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result!.NumsFalse.Should().BeNull();
    }

    // ── int[] with ArgsSplit PartsDividers = true ─────────────────────────────

    [Test]
    public void NumsTrue_MultipleOccurrences_ShouldCollect()
    {
        // Arrange – single value "1,2,3" split by per-gap dividers ",","," into int[] { 1, 2, 3 }
        var args = new[] { "--nums-true", "1,2,3" };

        var expected = new SplitExample
        {
            NumsTrue = [1, 2, 3]
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void NumsTrue_SingleElement_ShouldSucceed()
    {
        // Arrange – no divider consumed; single element
        var args = new[] { "--nums-true", "99" };

        var expected = new SplitExample
        {
            NumsTrue = [99]
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void NumsTrue_InvalidInt_ShouldFail()
    {
        // Arrange
        var args = new[] { "--nums-true", "bad" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void NumsTrue_NotPresent_ShouldBeNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result!.NumsTrue.Should().BeNull();
    }

    // ── Collection of tuples with PartsDividers = true ───────────────────────

    [Test]
    public void TuplePairs_MultipleOccurrences_ShouldSucceed()
    {
        // Arrange – each "--tpairs" value is split by "_" into (int, string)
        var args = new[] { "--tpairs", "1_hello", "--tpairs", "2_world" };

        var expected = new SplitExample
        {
            TuplePairs = [(1, "hello"), (2, "world")]
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void TuplePairs_SingleOccurrence_ShouldSucceed()
    {
        // Arrange
        var args = new[] { "--tpairs", "42_foo" };

        var expected = new SplitExample
        {
            TuplePairs = [(42, "foo")]
        };

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void TuplePairs_MissingDivider_ShouldFail()
    {
        // Arrange – divider "_" is absent
        var args = new[] { "--tpairs", "1hello" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void TuplePairs_InvalidInt_ShouldFail()
    {
        // Arrange
        var args = new[] { "--tpairs", "notint_hello" };

        // Act
        var (_, errors, position) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        errors.Should().NotBeNull();
        position.Should().Be(2);
    }

    [Test]
    public void TuplePairs_NotPresent_ShouldBeNull()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (result, _, _) = ArgumentsReader.ToObject<SplitExample>(args);

        // Assert
        result!.TuplePairs.Should().BeNull();
    }
}