using One.Base.Helpers.DataProcessHelpers;

using System.Text;
using System.Text.RegularExpressions;

namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>所有的算子</summary>
public static class OperationsContainer
{
    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static string String2Hex(string input) => StringHelper.ConvertRealStringToHexString(input);

    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static string Hex2String(string input) => StringHelper.ConvertHexStringToRealString(input);

    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static string String2Unicode(string source)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(source);
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i += 2)
        {
            stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
        }
        return stringBuilder.ToString();
    }

    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static string Unicode2String(string source)
    {
        return new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(source, x => System.Convert.ToChar(System.Convert.ToUInt16(x.Result("1"), 16)).ToString());
    }

    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static string String2Base64(string source)
    {
        return StringHelper.Base64Encode(source);
    }

    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static string Base642String(string source)
    {
        return StringHelper.Base64DecodeToString(source);
    }
}

public sealed class OperationAttribute : Attribute
{
    public uint MaxInput { get; set; }
    public uint MinInput { get; set; }

    /// <summary>是否显示输入参数名</summary>
    public bool GenerateInputNames { get; set; }
}