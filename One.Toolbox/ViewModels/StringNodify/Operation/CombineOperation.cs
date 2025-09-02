namespace One.Toolbox.ViewModels.StringNodify.Operation;

internal class CombineOperation : IOperation
{
    private readonly Func<string[], string> _func;

    public CombineOperation(Func<string[], string> func) => _func = func;

    public string Execute(params string[] operands)
    {
        return _func.Invoke(operands);
    }
}