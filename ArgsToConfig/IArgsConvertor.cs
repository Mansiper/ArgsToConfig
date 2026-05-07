namespace ArgsToConfig;

/// <summary>
/// Implement this interface to provide a custom string-to-value convertor for use with <see cref="Attributes.ArgsConvertorAttribute"/>.
/// </summary>
public interface IArgsConvertor
{
    object Convert(string value);
}
