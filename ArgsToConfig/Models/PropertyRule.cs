using System.Reflection;

namespace ArgsToConfig.Models;

internal sealed class PropertyRule
{
    public PropertyInfo Property { get; init; } = null!;

    // ArgsHasParameter
    public string[]? HasParameterNames { get; init; }
    public int HasParameterPosition { get; init; } = -1;

    // ArgsValueFor
    public string[]? ValueForNames { get; init; }
    public bool ValueForOptional { get; init; }
    public string? ValueForDefault { get; init; }

    // ArgsValueForBool
    public string[]? ValueForBoolTrueNames { get; init; }
    public string[]? ValueForBoolFalseNames { get; init; }

    // ArgsEnum (property is an enum type)
    public bool IsEnum { get; init; }

    // ArgsAfter
    public string[]? AfterFields { get; init; }

    // ArgsOneOf
    public string[]? OneOfFields { get; init; }

    // ArgsIfSet
    public string[]? IfSetFields { get; init; }

    // ArgsPathspec
    public bool IsPathspec { get; init; }

    // Enum member rules (only used when IsEnum + enum-level ArgsValueFor)
    public EnumMemberRule[]? EnumMemberRules { get; init; }

    // ArgsObject (sub-object dispatch)
    public bool IsObject { get; init; }
    public string? ObjectRootName { get; init; }

    // ArgsPipeline (array of interface instances)
    public bool IsPipeline { get; init; }
    public Type? PipelineElementType { get; init; }

    // ArgsTuple (split value into tuple components)
    public string[]? TupleDividers { get; init; }
    public bool TuplePartsDividers { get; init; }

    // Implicit positional (no attributes – filled from positional args in order)
    public bool IsImplicitPositional { get; init; }

    // ArgsPositional (explicit positional index)
    public int PositionalIndex { get; init; } = -1;

    // ArgsConvertor (custom convertor type)
    public Type? ConvertorType { get; init; }

    // ArgsExistingOnlyFile
    public bool IsExistingOnlyFile { get; init; }

    // ArgsExistingOnlyDirectory
    public bool IsExistingOnlyDirectory { get; init; }

    // ArgsLegalFileNamesOnly
    public bool IsLegalFileNamesOnly { get; init; }

    // ArgsAcceptFromAmong
    public string[]? AcceptFromAmong { get; init; }
}