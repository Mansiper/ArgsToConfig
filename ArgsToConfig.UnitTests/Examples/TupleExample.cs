using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// --dsc  "1.5_hello.x"      => (double, string, char)    dividers: "_", "."
// --bib  "true:99:255"      => (bool, int, byte)         dividers: ":", ":"
// --idcb "7;3.14;A;false"   => (int, double, char, bool) dividers: ";", ";", ";"
// --mix  "hi|2|9.9|z|true"  => (string, byte, double, char, bool) dividers: "|", "|", "|", "|"

internal class TupleExample
{
    [ArgsValueFor("--dsc")]
    [ArgsTuple("_", ".")]
    public (double, string, char)? DoubleStringChar { get; set; }

    [ArgsValueFor("--bib")]
    [ArgsTuple(":", ":")]
    public (bool, int, byte)? BoolIntByte { get; set; }

    [ArgsValueFor("--idcb")]
    [ArgsTuple(";", ";", ";")]
    public (int, double, char, bool)? IntDoubleCharBool { get; set; }

    [ArgsValueFor("--mix")]
    [ArgsTuple("|", "|", "|", "|")]
    public (string, byte, double, char, bool)? Mix { get; set; }
}