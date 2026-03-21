using One.Base.Helpers.DataProcessHelpers;

using System.Text.RegularExpressions;

namespace One.Base.Tests;

public class RegexHelperTests
{
    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    public void IPv4Regex_ShouldMatch_ValidAddress(string ip)
    {
        Assert.Matches(RegexHelper.IPv4AddressRegex, ip);
    }

    [Theory]
    [InlineData("256.1.1.1")]
    [InlineData("192.168.1")]
    public void IPv4Regex_ShouldNotMatch_InvalidAddress(string ip)
    {
        Assert.DoesNotMatch(new Regex(RegexHelper.IPv4AddressRegex), ip);
    }

    [Theory]
    [InlineData("00:11:22:33:44:55")]
    [InlineData("0011.2233.4455")]
    public void MacRegex_ShouldMatch_ValidAddress(string mac)
    {
        Assert.Matches(RegexHelper.MACAddressRegex, mac);
    }
}
