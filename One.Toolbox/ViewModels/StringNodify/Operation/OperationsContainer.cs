using One.Base.Helpers.DataProcessHelpers;

using System.Text;

namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>所有的算子</summary>
public static class OperationsContainer
{
    #region Input

    //public static byte[] ParseInput(string input) => Encoding.Default.GetBytes(input);

    #endregion Input

    #region Output

    //[Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    //public static string ParseOutput(byte[] input) => Encoding.Default.GetString(input);

    #endregion Output

    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static byte[] String2Hex(byte[] input) => Encoding.Default.GetBytes(StringHelper.BytesToHexString(input));

    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static byte[] Hex2String(byte[] input) => StringHelper.HexStringToBytes(Encoding.Default.GetString(input));
}

public sealed class OperationAttribute : Attribute
{
    public uint MaxInput { get; set; }
    public uint MinInput { get; set; }

    /// <summary>是否显示输入参数名</summary>
    public bool GenerateInputNames { get; set; }
}