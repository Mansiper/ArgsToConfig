using ArgsToConfig;
using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

/// <summary>Converts an IPv4 address string (e.g. "192.168.1.1") to int[].</summary>
public class IPv4ToIntArrayConvertor : IArgsConvertor
{
    public object Convert(string value)
    {
        var parts = value.Split('.');
        if (parts.Length != 4)
            throw new ArgumentException($"'{value}' is not a valid IPv4 address.");
        var result = new int[4];
        for (var i = 0; i < 4; i++)
        {
            if (!int.TryParse(parts[i], out var octet) || octet < 0 || octet > 255)
                throw new ArgumentException($"Invalid octet '{parts[i]}' in IPv4 address '{value}'.");
            result[i] = octet;
        }
        return result;
    }
}

public class ConvertorExample
{
    [ArgsValueFor("--ip")]
    [ArgsConvertor(typeof(IPv4ToIntArrayConvertor))]
    public int[]? IpAddress { get; set; }
}
