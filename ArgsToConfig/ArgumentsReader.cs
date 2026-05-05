using System.Reflection;
using ArgsToConfig.Attributes;
using ArgsToConfig.Models;

namespace ArgsToConfig;

public static class ArgumentsReader
{
    public static T ToObject<T>(params string[] args) where T : new()
    {
        var obj = new T();
        var rules = BuildRules(typeof(T));
        ApplyRules(obj, rules, args);
        return obj;
    }

    private static List<PropertyRule> BuildRules(Type type)
    {
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var rules = new List<PropertyRule>(props.Length);

        foreach (var prop in props)
        {
            var hasParam = prop.GetCustomAttribute<ArgsHasParameterAttribute>();
            var valueFor = prop.GetCustomAttribute<ArgsValueForAttribute>();
            var valueForBool = prop.GetCustomAttribute<ArgsValueForBoolAttribute>();
            var isEnum = prop.GetCustomAttribute<ArgsEnumAttribute>() is not null;
            var after = prop.GetCustomAttribute<ArgsAfterAttribute>();
            var oneOf = prop.GetCustomAttribute<ArgsOneOfAttribute>();
            var ifSet = prop.GetCustomAttribute<ArgsIfSetAttribute>();
            var isPathspec = prop.GetCustomAttribute<ArgsPathspecAttribute>() is not null;
            var isObject = prop.GetCustomAttribute<ArgsObjectAttribute>() is not null;

            // ArgsObject – sub-object with ArgsObjectRoot on its type
            if (isObject)
            {
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var rootAttr = propType.GetCustomAttribute<ArgsObjectRootAttribute>();
                rules.Add(new PropertyRule
                {
                    Property = prop,
                    IsObject = true,
                    ObjectRootName = rootAttr?.GetName
                });
                continue;
            }

            // Enum type – build member rules
            EnumMemberRule[]? enumMemberRules = null;
            if (isEnum)
            {
                var enumType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (enumType.IsEnum)
                {
                    // Check if enum itself has ArgsValueFor
                    var enumValueFor = enumType.GetCustomAttribute<ArgsValueForAttribute>();
                    var members = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    enumMemberRules = members.Select(m =>
                    {
                        var mHas = m.GetCustomAttribute<ArgsHasParameterAttribute>();
                        var mVal = m.GetCustomAttribute<ArgsValueAttribute>();
                        return new EnumMemberRule
                        {
                            Value = m.GetValue(null)!,
                            HasParameterNames = mHas?.GetNames,
                            ArgsValue = mVal?.GetValue
                        };
                    }).ToArray();

                    // Promote enum-level ArgsValueFor to property rule
                    if (enumValueFor is not null && valueFor is null)
                        valueFor = enumValueFor;
                }
            }

            rules.Add(new PropertyRule
            {
                Property = prop,
                HasParameterNames = hasParam?.GetNames,
                HasParameterPosition = hasParam?.GetPosition ?? -1,
                ValueForNames = valueFor?.GetNames,
                ValueForOptional = valueFor?.GetOptional ?? false,
                ValueForDefault = valueFor?.DefaultValue,
                ValueForBoolTrueNames = valueForBool?.GetTrueNames,
                ValueForBoolFalseNames = valueForBool?.GetFalseNames,
                IsEnum = isEnum,
                AfterFields = after?.GetFields,
                OneOfFields = oneOf?.GetFields,
                IfSetFields = ifSet?.GetFields,
                IsPathspec = isPathspec,
                EnumMemberRules = enumMemberRules,
                IsImplicitPositional = hasParam is null && valueFor is null && valueForBool is null
                    && !isEnum && after is null && !isPathspec
            });
        }

        return rules;
    }

    private static void ApplyRules(object obj, List<PropertyRule> rules, string[] args)
    {
        // ── ArgsObject subcommand dispatch ───────────────────────────────────
        var objectRules = rules.Where(r => r.IsObject).ToList();
        if (objectRules.Count > 0)
        {
            // Find the subcommand keyword (first non-option positional arg)
            var subcmdIndex = -1;
            string? subcmdName = null;
            for (var si = 0; si < args.Length; si++)
            {
                if (!args[si].StartsWith('-'))
                {
                    subcmdIndex = si;
                    subcmdName = args[si];
                    break;
                }
            }

            if (subcmdName is null)
                throw new ArgumentException("No subcommand specified.");

            var matchedRule = objectRules.FirstOrDefault(r =>
                string.Equals(r.ObjectRootName, subcmdName, StringComparison.OrdinalIgnoreCase));

            if (matchedRule is null)
                throw new ArgumentException($"Unknown subcommand: '{subcmdName}'.");

            // Instantiate the sub-object and recurse
            var subType = Nullable.GetUnderlyingType(matchedRule.Property.PropertyType)
                          ?? matchedRule.Property.PropertyType;
            var subObj = Activator.CreateInstance(subType)!;
            var subArgs = args[(subcmdIndex + 1)..];
            var subRules = BuildRules(subType);
            ApplyRules(subObj, subRules, subArgs);
            matchedRule.Property.SetValue(obj, subObj);

            // All other [ArgsObject] properties stay null (already default)
            return;
        }

        args = ExpandCombinedShortFlags(args, rules);

        var setFieldNames = new HashSet<string>();
        var knownArgNames = BuildKnownArgNames(rules);
        var endOfOptionsIndex = Array.IndexOf(args, "--");
        var consumedAt = new Dictionary<string, int>();
        var positionalArgs = new List<(int index, string value)>();

        // Build a flat lookup: arg-name → (rule, forced bool for ValueForBool, enum member for enum flags)
        var argIndex = new Dictionary<string, (PropertyRule Rule, bool? BoolValue, EnumMemberRule? Member)>(StringComparer.Ordinal);
        foreach (var rule in rules)
        {
            if (rule.ValueForBoolTrueNames is not null && rule.ValueForBoolFalseNames is not null)
            {
                foreach (var n in rule.ValueForBoolTrueNames)  argIndex[n] = (rule, true,  null);
                foreach (var n in rule.ValueForBoolFalseNames) argIndex[n] = (rule, false, null);
            }
            else if (rule.HasParameterNames is not null && rule.HasParameterPosition < 0)
            {
                foreach (var n in rule.HasParameterNames) argIndex[n] = (rule, null, null);
            }
            else if (rule.IsEnum && rule.EnumMemberRules is not null && rule.ValueForNames is null)
            {
                foreach (var mr in rule.EnumMemberRules)
                    if (mr.HasParameterNames is not null)
                        foreach (var n in mr.HasParameterNames) argIndex[n] = (rule, null, mr);
            }
            else if (rule.ValueForNames is not null)
            {
                foreach (var n in rule.ValueForNames) argIndex[n] = (rule, null, null);
            }
        }

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];

            if (a == "--") continue;

            if (endOfOptionsIndex >= 0 && i > endOfOptionsIndex)
            {
                positionalArgs.Add((i, a));
                continue;
            }

            if (!a.StartsWith('-'))
            {
                positionalArgs.Add((i, a));
                continue;
            }

            // Derive the lookup key: for --key=value or -kVALUE strip the value part
            var key = a;
            string? inlineValue = null;

            var eqPos = a.IndexOf('=');
            if (eqPos > 0)
            {
                key = a[..eqPos];
                inlineValue = a[(eqPos + 1)..];
            }
            else if (a.Length > 2 && !a.StartsWith("--"))
            {
                // Could be -kVALUE — try both the full arg and the two-char prefix
                if (!argIndex.ContainsKey(a))
                {
                    var shortKey = a[..2];
                    if (argIndex.ContainsKey(shortKey))
                    {
                        key = shortKey;
                        inlineValue = a[2..];
                    }
                }
            }

            if (!argIndex.TryGetValue(key, out var entry))
            {
                if (!knownArgNames.Contains(key))
                    throw new ArgumentException($"Unknown argument: '{a}'.");
                continue;
            }

            var (rule, boolValue, member) = entry;

            // ── ArgsValueForBool ──────────────────────────────────────────────
            if (boolValue is not null)
            {
                SetTracked(rule.Property, boolValue.Value, i);
                continue;
            }

            // ── ArgsHasParameter (bool flag) ──────────────────────────────────
            if (member is null && rule.ValueForNames is null)
            {
                if ((Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType) == typeof(bool))
                    SetTracked(rule.Property, true, i);
                continue;
            }

            // ── ArgsEnum with per-member ArgsHasParameter ─────────────────────
            if (member is not null)
            {
                SetTracked(rule.Property, member.Value, i);
                continue;
            }

            // ── ArgsValueFor (and ArgsEnum backed by ArgsValueFor) ────────────
            var value = inlineValue;
            if (value is null)
            {
                var next = i + 1 < args.Length ? args[i + 1] : null;
                if (next is not null && !next.StartsWith('-'))
                {
                    value = next;
                    i++;
                }
                else if (next is not null && next.StartsWith('-') && !next.StartsWith("\"") && !next.StartsWith("'"))
                    throw new ArgumentException($"Argument '{key}' requires a value but got '{next}' instead.");
                else if (next is null && !rule.ValueForOptional)
                    throw new ArgumentException($"Argument '{key}' requires a value but none was provided.");
            }

            var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;

            if (rule.IsEnum && rule.EnumMemberRules is not null)
            {
                if (value is not null)
                {
                    var enumMatched = false;
                    foreach (var mr in rule.EnumMemberRules)
                    {
                        var memberValue = mr.ArgsValue ?? mr.Value.ToString()!;
                        if (!string.Equals(memberValue, value, StringComparison.OrdinalIgnoreCase)) continue;
                        SetTracked(rule.Property, mr.Value, i);
                        enumMatched = true;
                        break;
                    }
                    if (!enumMatched)
                        throw new ArgumentException($"Invalid value '{value}' for argument '{key}'.");
                }
            }
            else if (propType == typeof(string[]))
            {
                if (value is not null)
                {
                    var existing = (string[]?)rule.Property.GetValue(obj);
                    string[] updated = existing is null ? [value] : [..existing, value];
                    rule.Property.SetValue(obj, updated);
                    setFieldNames.Add(rule.Property.Name);
                    consumedAt[rule.Property.Name] = i;
                }
            }
            else if (value is not null)
            {
                SetTracked(rule.Property, ConvertValue(value, propType), i);
            }
        }

        // ── Post-processing: one pass over rules ─────────────────────────────
        foreach (var rule in rules)
        {
            var name = rule.Property.Name;

            // Apply default for non-nullable enum properties not seen in args
            if (!setFieldNames.Contains(name)
                && rule.ValueForDefault is not null
                && rule.EnumMemberRules is not null
                && rule.ValueForNames is not null
                && Nullable.GetUnderlyingType(rule.Property.PropertyType) is null)
            {
                foreach (var mr in rule.EnumMemberRules)
                {
                    var memberValue = mr.ArgsValue ?? mr.Value.ToString()!;
                    if (!string.Equals(memberValue, rule.ValueForDefault, StringComparison.OrdinalIgnoreCase)) continue;
                    SetTracked(rule.Property, mr.Value, -1);
                    break;
                }
            }

            // ArgsHasParameter matched by positional value
            if (rule.HasParameterNames is not null && rule.HasParameterPosition >= 0)
            {
                var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
                if (propType == typeof(bool)
                    && positionalArgs.Any(p => rule.HasParameterNames.Any(n =>
                        string.Equals(n, p.value, StringComparison.OrdinalIgnoreCase))))
                    SetTracked(rule.Property, true, -1);
            }

            // ArgsPathspec
            if (rule.IsPathspec && endOfOptionsIndex >= 0)
            {
                var pathspecValues = args[(endOfOptionsIndex + 1)..];
                if (pathspecValues.Length > 0)
                    SetTracked(rule.Property, pathspecValues, -1);
            }

            // ArgsAfter
            if (rule.AfterFields is not null)
            {
                var minIndex = -1;
                foreach (var fieldName in rule.AfterFields)
                    if (consumedAt.TryGetValue(fieldName, out var idx) && idx > minIndex)
                        minIndex = idx;

                var candidate = positionalArgs.FirstOrDefault(p => p.index > minIndex);
                if (candidate.value is not null)
                {
                    var propType = Nullable.GetUnderlyingType(rule.Property.PropertyType) ?? rule.Property.PropertyType;
                    SetTracked(rule.Property, ConvertValue(candidate.value, propType), candidate.index);
                }
            }
        }

        // ── Implicit positional assignment ────────────────────────────────────
        var implicitRules = rules.Where(r => r.IsImplicitPositional && !setFieldNames.Contains(r.Property.Name)).ToList();
        var unconsumedPositionals = positionalArgs
            .Where(p => !consumedAt.Values.Contains(p.index))
            .ToList();
        for (var pi = 0; pi < implicitRules.Count && pi < unconsumedPositionals.Count; pi++)
        {
            var ipRule = implicitRules[pi];
            var (pidx, pval) = unconsumedPositionals[pi];
            var propType = Nullable.GetUnderlyingType(ipRule.Property.PropertyType) ?? ipRule.Property.PropertyType;
            SetTracked(ipRule.Property, ConvertValue(pval, propType), pidx);
        }

        // ── Remaining validation ──────────────────────────────────────────────
        foreach (var rule in rules)
        {
            var name = rule.Property.Name;

            // ArgsOneOf validation
            if (rule.OneOfFields is not null && setFieldNames.Contains(name))
            {
                var alsoSet = rule.OneOfFields.Where(f => setFieldNames.Contains(f)).ToList();
                if (alsoSet.Count > 0)
                    throw new InvalidOperationException(
                        $"Property '{name}' and '{string.Join("', '", alsoSet)}' are mutually exclusive ([ArgsOneOf]).");
            }

            // ArgsIfSet validation
            if (rule.IfSetFields is not null && setFieldNames.Contains(name))
            {
                foreach (var required in rule.IfSetFields)
                    if (!setFieldNames.Contains(required))
                        throw new ArgumentException(
                            $"Property '{name}' requires '{required}' to be set.");
            }
        }

        return;

        void SetTracked(PropertyInfo prop, object value, int argIndex)
        {
            if (setFieldNames.Contains(prop.Name))
                throw new ArgumentException($"Argument for '{prop.Name}' was specified more than once.");

            prop.SetValue(obj, value);
            setFieldNames.Add(prop.Name);
            if (argIndex >= 0)
                consumedAt[prop.Name] = argIndex;
        }
    }

    private static HashSet<string> BuildKnownArgNames(List<PropertyRule> rules)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
        {
            if (rule.HasParameterNames is not null)
                foreach (var n in rule.HasParameterNames) names.Add(n);
            if (rule.ValueForNames is not null)
                foreach (var n in rule.ValueForNames) names.Add(n);
            if (rule.ValueForBoolTrueNames is not null)
                foreach (var n in rule.ValueForBoolTrueNames) names.Add(n);
            if (rule.ValueForBoolFalseNames is not null)
                foreach (var n in rule.ValueForBoolFalseNames) names.Add(n);
            if (rule.EnumMemberRules is not null)
                foreach (var mr in rule.EnumMemberRules)
                    if (mr.HasParameterNames is not null)
                        foreach (var n in mr.HasParameterNames) names.Add(n);
        }
        return names;
    }

    // Expand combined short flags: -am → -a -m (last flag can have a value)
    private static string[] ExpandCombinedShortFlags(string[] args, List<PropertyRule> rules)
    {
        // Collect all single-char flag names
        var singleCharFlags = new HashSet<char>();
        var valueFlags = new HashSet<char>(); // flags that take a value

        foreach (var rule in rules)
        {
            CollectSingleCharFlags(rule.HasParameterNames, singleCharFlags, null);
            CollectSingleCharFlags(rule.ValueForNames, singleCharFlags, valueFlags);
            CollectSingleCharFlags(rule.ValueForBoolTrueNames, singleCharFlags, null);
            CollectSingleCharFlags(rule.ValueForBoolFalseNames, singleCharFlags, null);
            if (rule.EnumMemberRules is not null)
                foreach (var mr in rule.EnumMemberRules)
                    CollectSingleCharFlags(mr.HasParameterNames, singleCharFlags, null);
        }

        var result = new List<string>();
        foreach (var arg in args)
        {
            // Combined short flags: start with '-', not '--', length > 2
            if (arg.StartsWith('-') && !arg.StartsWith("--") && arg.Length > 2 && !arg.Contains('='))
            {
                var chars = arg[1..];
                var expanded = false;
                // Check all but the last are known single-char flags
                var allKnown = chars.All(c => singleCharFlags.Contains(c) || valueFlags.Contains(c));
                if (allKnown && chars.Length > 1)
                {
                    expanded = true;
                    for (var ci = 0; ci < chars.Length - 1; ci++)
                        result.Add($"-{chars[ci]}");
                    // Last char: if it's a value flag, add it alone (value comes next)
                    result.Add($"-{chars[^1]}");
                }
                if (!expanded) result.Add(arg);
            }
            else
            {
                result.Add(arg);
            }
        }
        return result.ToArray();

        static void CollectSingleCharFlags(string[]? names, HashSet<char> flags, HashSet<char>? valueSet)
        {
            if (names is null) return;
            foreach (var n in names)
                if (n.Length == 2 && n[0] == '-')
                {
                    flags.Add(n[1]);
                    valueSet?.Add(n[1]);
                }
        }
    }

    private static object ConvertValue(string raw, Type targetType)
    {
        // Strip surrounding quotes
        if (raw is ['"', _, ..] && raw[^1] == '"')
            raw = raw[1..^1];

        try
        {
            if (targetType == typeof(string)) return raw;
            if (targetType == typeof(bool)) return bool.Parse(raw);
            if (targetType == typeof(int)) return int.Parse(raw);
            if (targetType == typeof(DateTime)) return DateTime.Parse(raw);
            if (targetType.IsEnum) return Enum.Parse(targetType, raw, ignoreCase: true);
            return Convert.ChangeType(raw, targetType);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
        {
            throw new ArgumentException($"Invalid value '{raw}' for type '{targetType.Name}'.", ex);
        }
    }

    private static void SetProperty<T>(T obj, PropertyInfo prop, object value) => 
        prop.SetValue(obj, value);
}