namespace OutlookOtpClipboard.Tests;

public sealed class OtpExtractorTests
{
    private readonly OtpExtractor extractor = new();
    private readonly AppSettings settings = new();

    [Theory]
    [InlineData("Your code is 123456", "", "123456")]
    [InlineData("", "Use verification code 654321 to sign in.", "654321")]
    [InlineData("", "Code: 1234", "1234")]
    [InlineData("", "Use 12345678", "12345678")]
    public void Extract_ReturnsFirstValidCode(string subject, string body, string expected) =>
        Assert.Equal(expected, extractor.Extract(subject, body, settings));

    [Fact]
    public void Extract_IgnoresNumbersOutsideConfiguredRange()
    {
        Assert.Null(extractor.Extract("", "Order 123 and reference 123456789", settings));
    }

    [Fact]
    public void Extract_UsesCustomPattern()
    {
        settings.RegexPattern = @"\b[A-Z]{2}\d{4}\b";
        Assert.Equal("AB1234", extractor.Extract("", "Your code: AB1234", settings));
    }
}
