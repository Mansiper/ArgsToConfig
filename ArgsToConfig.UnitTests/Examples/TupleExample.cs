using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

// PartsDividers = true: each divider separates the corresponding pair of consecutive parts
// --dsc  "1.5_hello.x"      => (double, string, char)    dividers: "_", "."
// --bib  "true:99:255"      => (bool, int, byte)         dividers: ":", ":"
// --idcb "7;3.14;A;false"   => (int, double, char, bool) dividers: ";", ";", ";"
// --mix  "hi|2|9.9|z|true"  => (string, byte, double, char, bool) dividers: "|", "|", "|", "|"

// PartsDividers = false (default): all parts are separated by the same dividers applied cyclically
// --sc2  "hello;42"         => (string, int)             divider: ";"
// --i3   "1,2,3"            => (int, int, int)           divider: ","
// --alt  "a-1:b"            => (string, int, string)     dividers: "-", ":" (cyclic)

internal class TupleExample
{
    [ArgsValueFor("--dsc")]
    [ArgsTuple("_", ".", PartsDividers = true)]
    public (double, string, char)? DoubleStringChar { get; set; }

    [ArgsValueFor("--bib")]
    [ArgsTuple(":", ":", PartsDividers = true)]
    public (bool, int, byte)? BoolIntByte { get; set; }

    [ArgsValueFor("--idcb")]
    [ArgsTuple(";", ";", ";", PartsDividers = true)]
    public (int, double, char, bool)? IntDoubleCharBool { get; set; }

    [ArgsValueFor("--mix")]
    [ArgsTuple("|", "|", "|", "|", PartsDividers = true)]
    public (string, byte, double, char, bool)? Mix { get; set; }

    [ArgsValueFor("--sc2")]
    [ArgsTuple(";")]
    public (string, int)? StringInt { get; set; }

    [ArgsValueFor("--i3")]
    [ArgsTuple(",")]
    public (int, int, int)? ThreeInts { get; set; }

    [ArgsValueFor("--alt")]
    [ArgsTuple("-", ":")]
    public (string, int, string)? AltDividers { get; set; }
}