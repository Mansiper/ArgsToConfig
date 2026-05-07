using ArgsToConfig.UnitTests.Examples;
using FluentAssertions;

namespace ArgsToConfig.UnitTests;

[TestFixture]
public class CommonTypesTests
{
    [Test]
    public void CommonTypes_AllProvided_ShouldParseCorrectly()
    {
        // Arrange
        var args = new[]
        {
            "--date", "2024-06-15T14:30:00",
            "--only-date", "2024-06-15",
            "--time", "14:30:00",
            "--span", "01:30:00",
            "--file", "notes.txt",
            "--url", "https://example.com",
            "--guid", "d3b07384-d9a5-4e7d-a8d0-1234567890ab",
            "--ip", "192.168.1.1",
            "--version", "1.2.3.4"
        };

        // Act
        var result = ArgumentsReader.ToObject<CommonTypesExample>(args);

        // Assert
        result.Date.Should().Be(new DateTime(2024, 6, 15, 14, 30, 0));
        result.OnlyDate.Should().Be(new DateOnly(2024, 6, 15));
        result.Time.Should().Be(new TimeOnly(14, 30, 0));
        result.Span.Should().Be(new TimeSpan(1, 30, 0));
        result.File!.Name.Should().Be("notes.txt");
        result.Url.Should().Be(new Uri("https://example.com"));
        result.Guid.Should().Be(new Guid("d3b07384-d9a5-4e7d-a8d0-1234567890ab"));
        result.Ip.Should().Be(System.Net.IPAddress.Parse("192.168.1.1"));
        result.Version.Should().Be(new Version(1, 2, 3, 4));
    }
}