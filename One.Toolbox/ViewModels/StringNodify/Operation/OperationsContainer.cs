using One.Base.Helpers.DataProcessHelpers;

using System.Text;

namespace One.Toolbox.ViewModels.StringNodify;

/// <summary>所有的算子</summary>
public static class OperationsContainer
{
    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static string String2Hex(string input) => StringHelper.ConvertRealStringToHexString(input);

    [Operation(MinInput = 1, MaxInput = 1, GenerateInputNames = false)]
    public static string Hex2String(string input) => StringHelper.ConvertHexStringToRealString(input);
}

public sealed class OperationAttribute : Attribute
{
    public uint MaxInput { get; set; }
    public uint MinInput { get; set; }

    /// <summary>是否显示输入参数名</summary>
    public bool GenerateInputNames { get; set; }
}