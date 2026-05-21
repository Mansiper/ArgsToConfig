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
                if (rule.ObjectRootName is not null)
                    deferred.Add(rule.ObjectRootName[0]);
                BuildArgs(rawValue, deferred);
                continue;
            }

            // ── ArgsPipeline ─────────────────────────────────────────────────
            if (rule.IsPipeline)
            {
                var items = (System.Collections.IEnumerable)rawValue;
                foreach (var item in items)
                {
                    if (item is null)
                        continue;

                    var cmdAttr = item.GetType().GetCustomAttribute<ArgsPipelineCommandAttribute>();
                    if (cmdAttr is not null)
                        deferred.Add(cmdAttr.Name);
                    BuildArgs(item, deferred);
                }
                continue;
            }

            // ── ArgsValueForBool ─────────────────────────────────────────────
            if (rule.ValueForBoolTrueNames is not null && rule.ValueForBoolFalseNames is not null)
            {
                result.Add((bool)rawValue ? rule.ValueForBoolTrueNames[0] : rule.ValueForBoolFalseNames[0]);
                continue;
            }

            // ── ArgsHasParameter (bool flag) ──────────────────────────────────
            if (rule.HasParameterNames is not null && rule.ValueForNames is null)
            {
                var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
                if (propType == typeof(bool) && (bool)rawValue)
                {
                    var flagName = rule.HasParameterNames[0];
                    if (flagName.StartsWith('-'))
                        result.Add(flagName);
                    else
                        positionalRules.Add((rule.PositionalOrder, flagName));
                }
                continue;
            }

            // ── ArgsEnum ─────────────────────────────────────────────────────
            if (rule is { IsEnum: true, EnumMemberRules: not null })
            {
                var enumVal = rawValue;
                if (rule.ValueForNames is null)
                {
                    // Per-member ArgsHasParameter (no ValueFor)
                    var mr = rule.EnumMemberRules.FirstOrDefault(m => m.Value.Equals(enumVal));
                    if (mr?.ArgsEnumValue is not null)
                        result.Add(mr.ArgsEnumValue[0]);
                }
                else
                {
                    // Backed by ArgsValueFor: emit --flag value
                    var mr = rule.EnumMemberRules.FirstOrDefault(m => m.Value.Equals(enumVal));
                    if (mr is not null)
                    {
                        result.Add(rule.ValueForNames[0]);
                        result.Add(mr.ArgsEnumValue?[0] ?? mr.Value.ToString()!);
                    }
                }
                continue;
            }

            // ── ArgsValueFor ─────────────────────────────────────────────────
            if (rule.ValueForNames is not null)
            {
                var flagName = rule.ValueForNames[0];
                foreach (var sv in SerializePropertyValue(rule, rawValue))
                {
                    result.Add(flagName);
                    result.Add(sv);
                }
                continue;
            }

            // ── Implicit positional / ArgsPositional / ArgsAfter ─────────────
            if (rule.IsPositional)
            {
                foreach (var sv in SerializePropertyValue(rule, rawValue))
                    positionalRules.Add((rule.PositionalOrder, sv));
                continue;
            }

            // ── ArgsPathspec ──────────────────────────────────────────────────
            if (rule.IsPathspec)
            {
                result.Add("--");
                result.AddRange(SerializePropertyValue(rule, rawValue));
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
                    tokens.Add(rule.ObjectRootName[0]);
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
                tokens.Add($"[{rule.ValueForBoolTrueNames[0]} | {rule.ValueForBoolFalseNames[0]}]");
                continue;
            }

            // ── ArgsHasParameter (bool flag) ──────────────────────────────────
            if (rule.HasParameterNames is not null && rule.ValueForNames is null)
            {
                if (propType == typeof(bool))
                    tokens.Add($"[{string.Join(" | ", rule.HasParameterNames)}]");
                continue;
            }

            // ── ArgsEnum ─────────────────────────────────────────────────────
            if (rule is { IsEnum: true, EnumMemberRules: not null })
            {
                if (rule.ValueForNames is not null)
                {
                    var values = string.Join(" | ", rule.EnumMemberRules
                        .Select(m => m.ArgsEnumValue?[0] ?? m.Value.ToString()!));
                    var token = $"{rule.ValueForNames[0]} ({values})";
                    tokens.Add(rule.ValueForOptional ? $"[{token}]" : $"<{token}>");
                }
                else
                {
                    var names = string.Join(" | ", rule.EnumMemberRules
                        .Select(m => m.ArgsEnumValue?[0])
                        .Where(n => n is not null));
                    if (!string.IsNullOrEmpty(names))
                        tokens.Add($"[{names}]");
                }
                continue;
            }

            // ── ArgsValueFor ─────────────────────────────────────────────────
            if (rule.ValueForNames is not null)
            {
                var valueRepr = rule.SplitDividers is not null && InnerToObject.IsDictionaryProperty(propType, out _, out var dvType)
                    ? $"<key>{rule.SplitDividers[0]}<{dvType.Name.ToLowerInvariant()}>"
                    : rule.SplitDividers is not null
                        ? BuildTupleValueRepr(rule)
                        : $"<{propName}>";
                var token = $"{rule.ValueForNames[0]} {valueRepr}";
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
            if (rule.IsPositional)
                tokens.Add($"<{propName}>");
        }
    }

    private static string BuildTupleValueRepr(PropertyRule rule)
    {
        var underlyingType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
        var isCollection = InnerToObject.IsCollectionProperty(underlyingType, out var elementType);
        var tupleType = isCollection ? elementType : underlyingType;
        var typeArgs = tupleType.GetGenericArguments();
        var dividers = rule.SplitDividers!;

        var sb = new System.Text.StringBuilder();
        sb.Append($"<{typeArgs[0].Name.ToLowerInvariant()}>");
        for (var i = 1; i < typeArgs.Length; i++)
        {
            if (i < typeArgs.Length - 1 || dividers.Length < typeArgs.Length - 1)
                sb.Append(dividers[(i - 1) % dividers.Length]);
            else
                // Last divider — show alternatives if more than one divider was provided
                sb.Append(dividers.Length > 1 ? string.Join("|", dividers.Skip(i - 1)) : dividers[0]);
            sb.Append($"<{typeArgs[i].Name.ToLowerInvariant()}>");
        }

        return isCollection ? $"{sb}..." : sb.ToString();
    }

    private static List<string> SerializePropertyValue(PropertyRule rule, object value)
    {
        var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;

        if (InnerToObject.IsDictionaryProperty(propType, out _, out var valueType))
        {
            var divider = rule.SplitDividers?[0] ?? "=";
            var items = (System.Collections.IEnumerable)value;
            var result = new List<string>();
            foreach (System.Collections.DictionaryEntry entry in (System.Collections.IDictionary)items)
            {
                var keyStr = entry.Key.ToString() ?? string.Empty;
                var valStr = SerializeDictionaryValue(entry.Value, valueType, rule);
                result.Add($"{keyStr}{divider}{valStr}");
            }
            return result;
        }

        if (InnerToObject.IsCollectionProperty(propType, out var elementType))
        {
            var items = (System.Collections.IEnumerable)value;
            return [.. items.Cast<object>().Select(item => SerializeScalar(item, elementType, rule))];
        }

        return [SerializeScalar(value, propType, rule)];
    }

    private static string SerializeDictionaryValue(object? value, Type valueType, PropertyRule rule)
    {
        if (value is null) return string.Empty;
        var dividers = rule.SplitDividers is { Length: > 1 } d ? d[1..] : rule.SplitDividers;
        if (dividers is not null && valueType is { IsValueType: true, IsGenericType: true })
        {
            // Tuple value
            var fields = valueType.GetFields();
            var parts = new string[fields.Length];
            for (var i = 0; i < fields.Length; i++)
                parts[i] = fields[i].GetValue(value)?.ToString() ?? string.Empty;
            return string.Join(dividers[0], parts);
        }
        if (InnerToObject.IsCollectionProperty(valueType, out _))
        {
            var divider = dividers?[0] ?? ",";
            var items = (System.Collections.IEnumerable)value;
            return string.Join(divider, items.Cast<object>().Select(item => item?.ToString() ?? string.Empty));
        }
        return SerializeScalar(value, valueType, rule);
    }

    private static string SerializeScalar(object value, Type type, PropertyRule rule)
    {
        if (rule.SplitDividers is not null && type is { IsValueType: true, IsGenericType: true })
        {
            var fields = type.GetFields();
            var parts = new string[fields.Length];
            for (var i = 0; i < fields.Length; i++)
                parts[i] = fields[i].GetValue(value)?.ToString() ?? string.Empty;
            return string.Join(rule.SplitDividers[0], parts);
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