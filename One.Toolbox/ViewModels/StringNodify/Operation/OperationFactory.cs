using AvaloniaEdit.Utils;

using One.Toolbox.ViewModels.StringNodify.Operation;

using System.Reflection;

namespace One.Toolbox.ViewModels.StringNodify;

public static class OperationFactory
{
    public static List<OperationInfoVM> GetOperationsInfo(Type container)
    {
        List<OperationInfoVM> result = new();

        foreach (var method in container.GetMethods())
        {
            if (method.IsStatic)
            {
                OperationInfoVM op = new OperationInfoVM()
                {
                    Title = method.Name
                };

                var attr = method.GetCustomAttribute<OperationAttribute>();
                var para = method.GetParameters();

                bool generateInputNames = true;

                op.Type = OperationType.Normal;

                if (para.Length == 2)
                {
                }
                else if (para.Length == 1)
                {
                    if (para[0].ParameterType.IsArray)
                    {
                       
                    }
                    else
                    {
                        var delType = typeof(Func<string, string>);
                        var del = (Func<string, string>)Delegate.CreateDelegate(delType, method);

                        op.Operation = new TransportOperation(del);
                        op.MaxInput = int.MaxValue;
                    }
                }

                if (attr != null)
                {
                    op.MinInput = attr.MinInput;
                    op.MaxInput = attr.MaxInput;
                    generateInputNames = attr.GenerateInputNames;
                }
                else
                {
                    op.MinInput = (uint)para.Length;
                    op.MaxInput = (uint)para.Length;
                }

                foreach (var param in para)
                {
                    op.Input.Add(generateInputNames ? param.Name : null);
                }

                for (int i = op.Input.Count; i < op.MinInput; i++)
                {
                    op.Input.Add(null);
                }

                result.Add(op);
            }
        }

        return result;
    }

    public static OperationVM GetOperation(OperationInfoVM info)
    {
        var input = info.Input.Select(i => new ConnectorVM
        {
            Title = i
        });

        switch (info.Type)
        {
            case OperationType.Input:
                return new InputOperationVM
                {
                    Title = info.Title,
                   
                };

            case OperationType.Output:
                return new OutputOperationVM
                {
                    Title = info.Title,
                  
                };

            default:
                {
                    var op = new OperationVM
                    {
                        Title = info.Title,
                        Output = new ConnectorVM(),
                        Operation = info.Operation
                    };

                    op.Input.AddRange(input);
                    return op;
                }
        }
    }
}