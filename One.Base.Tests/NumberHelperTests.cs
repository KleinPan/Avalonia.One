using One.Base.Helpers.DataProcessHelpers;

namespace One.Base.Tests;

public class NumberHelperTests
{
    [Fact]
    public void EndianUINT16_ShouldSwapByteOrder()
    {
        var result = NumberHelper.EndianUINT16(0x1234);

        Assert.Equal((ushort)0x3412, result);
    }

    [Fact]
    public void EndianUINT32_ShouldSwapByteOrder()
    {
        var result = NumberHelper.EndianUINT32(0x12345678);

        Assert.Equal(0x78563412u, result);
    }

    [Fact]
    public void GetUINT32_ShouldReadBigEndianBuffer()
    {
        var result = NumberHelper.GetUINT32(new byte[] { 0x12, 0x34, 0x56, 0x78 });

        Assert.Equal(0x12345678u, result);
    }
}
