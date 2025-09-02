namespace One.Toolbox.ViewModels.StringNodify;

public interface IOperation
{
    /// <summary>所有的输入输出均为byte数组，一个byte[]代表一个输入或输出</summary>
    /// <param name="operands"></param>
    /// <returns></returns>
    string Execute(params string[] operands);
}