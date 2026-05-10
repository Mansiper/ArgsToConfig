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

// Collections of a plain type with ArgsSplit: a single value is split by dividers into elements
// --nums-false  "1,2,3"   => int[]   PartsDividers = false (cyclic divider ",")
// --nums-true   "1,2,3"   => int[]   PartsDividers = true  (explicit per-gap dividers)

// Collection of tuples with PartsDividers = true: each flag occurrence is split per-divider into one tuple
// --tpairs "1_hello" --tpairs "2_world"  => (int, string)[]   divider: "_"

internal class SplitExample
{
    [ArgsValueFor("--dsc")]
    [ArgsSplit("_", ".", PartsDividers = true)]
    public (double, string, char)? DoubleStringChar { get; set; }

    [ArgsValueFor("--bib")]
    [ArgsSplit(":", ":", PartsDividers = true)]
    public (bool, int, byte)? BoolIntByte { get; set; }

    [ArgsValueFor("--idcb")]
    [ArgsSplit(";", ";", ";", PartsDividers = true)]
    public (int, double, char, bool)? IntDoubleCharBool { get; set; }

    [ArgsValueFor("--mix")]
    [ArgsSplit("|", "|", "|", "|", PartsDividers = true)]
    public (string, byte, double, char, bool)? Mix { get; set; }

    [ArgsValueFor("--sc2")]
    [ArgsSplit(";")]
    public (string, int)? StringInt { get; set; }

    [ArgsValueFor("--i3")]
    [ArgsSplit(",")]
    public (int, int, int)? ThreeInts { get; set; }

    [ArgsValueFor("--alt")]
    [ArgsSplit("-", ":")]
    public (string, int, string)? AltDividers { get; set; }

    // Collection of tuples: --points "1,2" --points "3,4"  => (int, int)[]
    [ArgsValueFor("--points")]
    [ArgsSplit(",")]
    public (int, int)[]? Points { get; set; }

    // Collections of int split from a single value:
    // --nums-false "1,2,3"  =>  int[] { 1, 2, 3 }   PartsDividers = false (cyclic ",")
    [ArgsValueFor("--nums-false")]
    [ArgsSplit(",")]
    public int[]? NumsFalse { get; set; }

    // --nums-true "1,2,3"  =>  int[] { 1, 2, 3 }   PartsDividers = true (",", ",")
    [ArgsValueFor("--nums-true")]
    [ArgsSplit(",", ",", PartsDividers = true)]
    public int[]? NumsTrue { get; set; }

    // Collection of tuples with PartsDividers = true: each occurrence is split per-divider into one tuple
    [ArgsValueFor("--tpairs")]
    [ArgsSplit("_", PartsDividers = true)]
    public (int, string)[]? TuplePairs { get; set; }
}