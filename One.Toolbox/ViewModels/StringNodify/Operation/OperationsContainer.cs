using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace One.Toolbox.ViewModels.StringNodify;

public static class OperationsContainer
{
    public static byte[] String2Hex(byte[] input)
      => Encoding.Default.GetBytes(BitConverter.ToString(input).Replace("-", " "));

    public static byte[] Hex2String(byte[] input)
     => Hex2byte(Encoding.Default.GetString(input));

    public static byte[] Hex2byte(string mHex)
    {
        mHex = Regex.Replace(mHex, "[^0-9A-Fa-f]", "");
        if (mHex.Length % 2 != 0)
            mHex = mHex.Remove(mHex.Length - 1, 1);
        if (mHex.Length <= 0) return new byte[] { };
        byte[] vBytes = new byte[mHex.Length / 2];
        for (int i = 0; i < mHex.Length; i += 2)
            if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2]))
                vBytes[i / 2] = 0;
        return vBytes;
    }
}

public sealed class OperationAttribute : Attribute
{
    public uint MaxInput { get; set; }
    public uint MinInput { get; set; }
    public bool GenerateInputNames { get; set; }
}