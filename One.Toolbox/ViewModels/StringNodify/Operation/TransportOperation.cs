namespace One.Toolbox.ViewModels.StringNodify.Operation;

internal class TransportOperation : IOperation
{
    private readonly Func<string, string> _func;

    public TransportOperation(Func<string , string> func) => _func = func;

    public string Execute(params string[] operands)
    {
        return _func.Invoke(operands[0]);
    }
}