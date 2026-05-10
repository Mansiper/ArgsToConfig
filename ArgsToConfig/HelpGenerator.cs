// Copyright (c) 2026 Pavel Razboynikov
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ArgsToConfig.Attributes;
using System.Reflection;
using System.Text;

namespace ArgsToConfig;

/// <summary>
/// Generates and caches human-readable help text for a config type based on its attribute descriptions.
/// </summary>
public static class HelpGenerator
{
    private static readonly Dictionary<Type, string> Cache = new();

    /// <summary>
    /// Returns the help text for the given config type, generating and caching it on first call.
    /// </summary>
    public static string GetHelp<T>() => GetHelp(typeof(T));

    /// <summary>
    /// Returns the help text for the given config type, generating and caching it on first call.
    /// </summary>
    public static string GetHelp(Type type)
    {
        if (Cache.TryGetValue(type, out var cached))
            return cached;

        var help = Generate(type);
        Cache[type] = help;
        return help;
    }

    /// <summary>
    /// Clears the help text cache. Useful in tests or after dynamic reconfiguration.
    /// </summary>
    public static void ClearCache() => Cache.Clear();

    private static string Generate(Type type)
    {
        var sb = new StringBuilder();

        // Class-level OneOf descriptions
        var oneOfs = type.GetCustomAttributes<ArgsOneOfAttribute>();
        foreach (var oneOf in oneOfs)
            if (oneOf.Description is not null)
                sb.AppendLine($"Note: {oneOf.Description}");

        // Class-level MutuallyRequired descriptions
        var mutuallyRequireds = type.GetCustomAttributes<ArgsMutuallyRequiredAttribute>();
        foreach (var mutReq in mutuallyRequireds)
            if (mutReq.Description is not null)
                sb.AppendLine($"Note: {mutReq.Description}");

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Split into ungrouped (no ArgsHelpGroup) and grouped, preserving declaration order
        var ungrouped = props.Where(p => p.GetCustomAttribute<ArgsHelpGroupAttribute>() is null).ToList();
        var grouped = props
            .Select(p => (prop: p, group: p.GetCustomAttribute<ArgsHelpGroupAttribute>()))
            .Where(x => x.group is not null)
            .GroupBy(x => x.group!.Name, StringComparer.Ordinal)
            .ToList();

        foreach (var prop in ungrouped)
            AppendPropertyHelp(sb, prop);

        foreach (var group in grouped)
        {
            // Only emit the section header if at least one property in the group has help text
            var groupProps = group.Select(x => x.prop).ToList();
            var hasAny = groupProps.Any(p => HasHelpText(p));
            if (hasAny)
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.AppendLine($"{group.Key}:");
            }
            foreach (var prop in groupProps)
                AppendPropertyHelp(sb, prop);
        }

        return sb.ToString().TrimEnd();
    }

    private static bool HasHelpText(PropertyInfo prop)
    {
        var hasParam = prop.GetCustomAttribute<ArgsHasParameterAttribute>();
        var valueFor = prop.GetCustomAttribute<ArgsValueForAttribute>();
        var valueForBool = prop.GetCustomAttribute<ArgsValueForBoolAttribute>();
        var argsEnum = prop.GetCustomAttribute<ArgsEnumAttribute>();
        var argsObject = prop.GetCustomAttribute<ArgsObjectAttribute>();
        var argsPipeline = prop.GetCustomAttribute<ArgsPipelineAttribute>();
        var argsPathspec = prop.GetCustomAttribute<ArgsPathspecAttribute>();
        var argsPositional = prop.GetCustomAttribute<ArgsPositionalAttribute>();

        return (hasParam?.Description
                ?? valueFor?.Description
                ?? valueForBool?.Description
                ?? argsEnum?.Description
                ?? argsObject?.Description
                ?? argsPipeline?.Description
                ?? argsPathspec?.Description
                ?? argsPositional?.Description) is not null;
    }

    private static void AppendPropertyHelp(StringBuilder sb, PropertyInfo prop)
    {
        var hasParam = prop.GetCustomAttribute<ArgsHasParameterAttribute>();
        var valueFor = prop.GetCustomAttribute<ArgsValueForAttribute>();
        var valueForBool = prop.GetCustomAttribute<ArgsValueForBoolAttribute>();
        var argsEnum = prop.GetCustomAttribute<ArgsEnumAttribute>();
        var argsObject = prop.GetCustomAttribute<ArgsObjectAttribute>();
        var argsPipeline = prop.GetCustomAttribute<ArgsPipelineAttribute>();
        var argsPathspec = prop.GetCustomAttribute<ArgsPathspecAttribute>();
        var argsPositional = prop.GetCustomAttribute<ArgsPositionalAttribute>();
        var argsIfSet = prop.GetCustomAttribute<ArgsIfSetAttribute>();

        var description = hasParam?.Description
                          ?? valueFor?.Description
                          ?? valueForBool?.Description
                          ?? argsEnum?.Description
                          ?? argsObject?.Description
                          ?? argsPipeline?.Description
                          ?? argsPathspec?.Description
                          ?? argsPositional?.Description;

        if (description is null)
            return;

        var names = BuildNames(prop, hasParam, valueFor, valueForBool, argsEnum, argsObject, argsPathspec, argsPositional);

        sb.Append($"  {names}");

        // Append value placeholder for value-accepting args
        if (valueFor is not null)
        {
            var placeholder = $"<{prop.Name.ToLower()}>";
            if (valueFor.Optional)
                sb.Append($" [{placeholder}]");
            else
                sb.Append($" {placeholder}");
        }
        else if (argsEnum is not null && argsEnum.GetNames is not null)
        {
            var enumType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (enumType.IsEnum)
            {
                var values = string.Join("|", Enum.GetNames(enumType).Select(n => n.ToLower()));
                sb.Append($" <{values}>");
            }
        }

        sb.AppendLine($"\t{description}");

        // If this is an enum property with member descriptions, list them
        if (argsEnum is not null)
        {
            var enumType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (enumType.IsEnum)
            {
                var members = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var member in members)
                {
                    var mHas = member.GetCustomAttribute<ArgsHasParameterAttribute>();
                    var mVal = member.GetCustomAttribute<ArgsEnumValueAttribute>();
                    var memberDesc = mHas?.Description ?? mVal?.Description;
                    if (memberDesc is null) continue;
                    var memberName = mHas?.GetNames is { } n ? string.Join(", ", n) : (mVal?.GetValues is { } v ? string.Join(", ", v) : member.Name.ToLower());
                    sb.AppendLine($"      {memberName}\t{memberDesc}");
                }
            }
        }

        // Inline enum (ArgsHasParameter on enum members treated as flags)
        if (hasParam is null && valueFor is null && valueForBool is null && argsEnum is null)
        {
            // nothing extra
        }

        if (argsIfSet?.Description != null)
            sb.AppendLine($"      (requires: {string.Join(", ", argsIfSet.GetFields)})\t{argsIfSet.Description}");
    }

    private static string BuildNames(
        PropertyInfo prop,
        ArgsHasParameterAttribute? hasParam,
        ArgsValueForAttribute? valueFor,
        ArgsValueForBoolAttribute? valueForBool,
        ArgsEnumAttribute? argsEnum,
        ArgsObjectAttribute? argsObject,
        ArgsPathspecAttribute? argsPathspec,
        ArgsPositionalAttribute? argsPositional)
    {
        if (hasParam is not null)
            return string.Join(", ", hasParam.GetNames);
        if (valueFor is not null)
            return string.Join(", ", valueFor.GetNames);
        if (valueForBool is not null)
            return $"{string.Join(", ", valueForBool.GetTrueNames)} / {string.Join(", ", valueForBool.GetFalseNames)}";
        if (argsEnum?.GetNames is not null)
            return string.Join(", ", argsEnum.GetNames);
        if (argsObject is not null)
            return string.Join(", ", argsObject.GetNames);
        if (argsPathspec is not null)
            return "[--] <pathspec>...";
        if (argsPositional is not null)
            return $"<{prop.Name.ToLower()}> (positional {argsPositional.GetPosition})";
        return $"<{prop.Name.ToLower()}>";
    }
}