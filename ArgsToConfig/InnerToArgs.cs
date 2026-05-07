using ArgsToConfig.Attributes;
using ArgsToConfig.Models;
using System.Reflection;

namespace ArgsToConfig;

internal static class InnerToArgs
{
    internal static void BuildArgs(object obj, List<string> result)
    {
        var rules = InnerToObject.BuildRules(obj.GetType());
        var positionalRules = new List<(int order, string value)>();
        // Deferred pipeline/subobject segments emitted after all root args
        var deferred = new List<string>();

        foreach (var rule in rules)
        {
            var rawValue = rule.Property.GetValue(obj);
            if (rawValue is null)
                continue;

            // ── ArgsObject ───────────────────────────────────────────────────
            if (rule.IsObject)
            {
                deferred.Add(rule.ObjectRootName!);
                BuildArgs(rawValue, deferred);
                continue;
            }

            // ── ArgsPipeline ─────────────────────────────────────────────────
            if (rule.IsPipeline)
            {
                var items = (System.Collections.IEnumerable)rawValue;
                foreach (var item in items)
                {
                    var itemType = item.GetType();
                    var cmdAttr = itemType.GetCustomAttribute<ArgsPipelineCommandAttribute>();
                    if (cmdAttr is not null)
                        deferred.Add(cmdAttr.GetName);
                    BuildArgs(item, deferred);
                }
                continue;
            }

            // ── ArgsValueForBool ─────────────────────────────────────────────
            if (rule.ValueForBoolTrueNames is not null && rule.ValueForBoolFalseNames is not null)
            {
                var bVal = (bool)rawValue;
                result.Add(bVal ? rule.ValueForBoolTrueNames[0] : rule.ValueForBoolFalseNames[0]);
                continue;
            }

            // ── ArgsHasParameter (bool flag) ──────────────────────────────────
            if (rule.HasParameterNames is not null && rule.ValueForNames is null)
            {
                var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
                if (propType == typeof(bool))
                {
                    if ((bool)rawValue)
                    {
                        // Positional-name parameter (no dash) or dash flag
                        var flagName = rule.HasParameterNames[0];
                        if (!flagName.StartsWith('-'))
                            positionalRules.Add((rule.PositionalIndex >= 0 ? rule.PositionalIndex : int.MaxValue, flagName));
                        else
                            result.Add(flagName);
                    }
                    // continue;
                }
                continue;
            }

            // ── ArgsEnum ─────────────────────────────────────────────────────
            if (rule.IsEnum && rule.EnumMemberRules is not null)
            {
                var enumVal = rawValue;
                // Per-member ArgsHasParameter (no ValueFor)
                if (rule.ValueForNames is null)
                {
                    var mr = rule.EnumMemberRules.FirstOrDefault(m => m.Value.Equals(enumVal) && m.HasParameterNames is not null);
                    if (mr is not null)
                        result.Add(mr.HasParameterNames![0]);
                    continue;
                }
                // Backed by ArgsValueFor: emit --flag value
                var valueMr = rule.EnumMemberRules.FirstOrDefault(m => m.Value.Equals(enumVal));
                if (valueMr is not null)
                {
                    result.Add(rule.ValueForNames![0]);
                    result.Add(valueMr.ArgsValue ?? valueMr.Value.ToString()!);
                }
                continue;
            }

            // ── ArgsValueFor ─────────────────────────────────────────────────
            if (rule.ValueForNames is not null)
            {
                var strValues = SerializePropertyValue(rule, rawValue);
                foreach (var sv in strValues)
                {
                    result.Add(rule.ValueForNames[0]);
                    result.Add(sv);
                }
                continue;
            }

            // ── Implicit positional / ArgsPositional / ArgsAfter ─────────────
            if (rule.IsImplicitPositional || rule.PositionalIndex >= 0 || rule.AfterFields is not null)
            {
                var strValues = SerializePropertyValue(rule, rawValue);
                var order = rule.PositionalIndex >= 0 ? rule.PositionalIndex : int.MaxValue;
                foreach (var sv in strValues)
                    positionalRules.Add((order, sv));
                continue;
            }

            // ── ArgsPathspec ──────────────────────────────────────────────────
            if (rule.IsPathspec)
            {
                result.Add("--");
                var pathspecValues = SerializePropertyValue(rule, rawValue);
                result.AddRange(pathspecValues);
                // continue;
            }
        }

        // Emit positional values in declared order, then deferred subcommand/pipeline segments
        foreach (var (_, value) in positionalRules.OrderBy(p => p.order))
            result.Add(value);
        result.AddRange(deferred);
    }

    internal static void BuildArgsString(Type type, List<string> tokens)
    {
        var rules = InnerToObject.BuildRules(type);

        foreach (var rule in rules)
        {
            var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
            var propName = rule.Property.Name.ToLowerInvariant();

            // ── ArgsObject ───────────────────────────────────────────────────
            if (rule.IsObject)
            {
                if (rule.ObjectRootName is not null)
                    tokens.Add(rule.ObjectRootName);
                BuildArgsString(rule.Property.PropertyType, tokens);
                continue;
            }

            // ── ArgsPipeline ─────────────────────────────────────────────────
            if (rule.IsPipeline)
            {
                tokens.Add($"[<{propName}>...]");
                continue;
            }

            // ── ArgsValueForBool ─────────────────────────────────────────────
            if (rule.ValueForBoolTrueNames is not null && rule.ValueForBoolFalseNames is not null)
            {
                var trueFlag = rule.ValueForBoolTrueNames[0];
                var falseFlag = rule.ValueForBoolFalseNames[0];
                tokens.Add($"[{trueFlag} | {falseFlag}]");
                continue;
            }

            // ── ArgsHasParameter (bool flag) ──────────────────────────────────
            if (rule.HasParameterNames is not null && rule.ValueForNames is null)
            {
                if (propType == typeof(bool))
                {
                    var names = string.Join(" | ", rule.HasParameterNames);
                    tokens.Add(rule.HasParameterNames.Length > 1 ? $"[{names}]" : $"[{names}]");
                }
                continue;
            }

            // ── ArgsEnum ─────────────────────────────────────────────────────
            if (rule.IsEnum && rule.EnumMemberRules is not null)
            {
                if (rule.ValueForNames is not null)
                {
                    var flag = rule.ValueForNames[0];
                    var values = string.Join(" | ", rule.EnumMemberRules
                        .Select(m => m.ArgsValue ?? m.Value.ToString()!));
                    var optional = rule.ValueForOptional;
                    var token = $"{flag} ({values})";
                    tokens.Add(optional ? $"[{token}]" : $"<{token}>");
                }
                else
                {
                    var names = string.Join(" | ", rule.EnumMemberRules
                        .Where(m => m.HasParameterNames is not null)
                        .Select(m => m.HasParameterNames![0]));
                    if (!string.IsNullOrEmpty(names))
                        tokens.Add($"[{names}]");
                }
                continue;
            }

            // ── ArgsValueFor ─────────────────────────────────────────────────
            if (rule.ValueForNames is not null)
            {
                var flag = rule.ValueForNames[0];
                string valueRepr;
                if (rule.TupleDividers is not null)
                {
                    var underlyingType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
                    var isCollection = InnerToObject.IsCollectionProperty(underlyingType, out var elementType);
                    var tupleType = isCollection ? elementType! : underlyingType;
                    var typeArgs = tupleType.GetGenericArguments();
                    var dividers = rule.TupleDividers;
                    var parts = new System.Text.StringBuilder();
                    parts.Append($"<{typeArgs[0].Name.ToLowerInvariant()}>");
                    for (var i = 1; i < typeArgs.Length; i++)
                    {
                        var divider = dividers[(i - 1) % dividers.Length];
                        if (i < typeArgs.Length - 1 || dividers.Length < typeArgs.Length - 1)
                            parts.Append(divider);
                        else
                            // last divider — show alternatives if more than one divider was provided
                            parts.Append(dividers.Length > 1 ? $"{string.Join("|", dividers.Skip(i - 1))}" : divider);
                        parts.Append($"<{typeArgs[i].Name.ToLowerInvariant()}>");
                    }
                    valueRepr = isCollection ? $"{parts}..." : parts.ToString();
                }
                else
                {
                    valueRepr = $"<{propName}>";
                }
                var token = $"{flag} {valueRepr}";
                tokens.Add(rule.ValueForOptional ? $"[{token}]" : $"<{token}>");
                continue;
            }

            // ── ArgsPathspec ──────────────────────────────────────────────────
            if (rule.IsPathspec)
            {
                tokens.Add($"[-- <{propName}>...]");
                continue;
            }

            // ── Positional ────────────────────────────────────────────────────
            if (rule.IsImplicitPositional || rule.PositionalIndex >= 0 || rule.AfterFields is not null)
            {
                tokens.Add($"<{propName}>");
            }
        }
    }

    private static List<string> SerializePropertyValue(PropertyRule rule, object value)
    {
        var result = new List<string>();
        var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;

        if (InnerToObject.IsCollectionProperty(propType, out var elementType))
        {
            var items = (System.Collections.IEnumerable)value;
            foreach (var item in items)
                result.Add(SerializeScalar(item, elementType, rule));
            return result;
        }

        result.Add(SerializeScalar(value, propType, rule));
        return result;
    }

    private static string SerializeScalar(object value, Type type, PropertyRule rule)
    {
        if (rule.TupleDividers is not null && type.IsGenericType)
        {
            var typeArgs = type.GetGenericArguments();
            var fields = type.GetFields();
            var parts = new string[typeArgs.Length];
            for (var i = 0; i < typeArgs.Length; i++)
                parts[i] = fields[i].GetValue(value)?.ToString() ?? string.Empty;
            var divider = rule.TupleDividers[0];
            return string.Join(divider, parts);
        }

        return value switch
        {
            DateTime dt => dt.ToString("O"),
            DateOnly d => d.ToString("O"),
            TimeOnly t => t.ToString("O"),
            TimeSpan ts => ts.ToString("c"),
            Uri uri => uri.ToString(),
            FileInfo fi => fi.FullName,
            DirectoryInfo di => di.FullName,
            _ => value.ToString() ?? string.Empty
        };
    }
}