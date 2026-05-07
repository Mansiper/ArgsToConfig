using System.ComponentModel.DataAnnotations;
using ArgsToConfig.Attributes;

namespace ArgsToConfig.UnitTests.Examples;

public class ValidationExample
{
    [ArgsValueFor("--name")]
    [Required]
    public string Name { get; set; } = "";

    [ArgsValueFor("--email")]
    [EmailAddress]
    public string? Email { get; set; }

    [ArgsValueFor("--phone")]
    [Phone]
    public string? Phone { get; set; }

    [ArgsValueFor("--count")]
    [System.ComponentModel.DataAnnotations.Range(1, 100)]
    public int Count { get; set; }

    [ArgsValueFor("--tag")]
    [MaxLength(10)]
    public string? Tag { get; set; }

    [ArgsValueFor("--code")]
    [RegularExpression("^[A-Z]{2,5}$")]
    public string? Code { get; set; }
}
