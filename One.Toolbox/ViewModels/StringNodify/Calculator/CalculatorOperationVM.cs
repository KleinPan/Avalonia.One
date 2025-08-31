using Avalonia;

using One.Base.ExtensionMethods;
using One.Toolbox.ExtensionMethods;

using System.Windows;

namespace   One.Toolbox.ViewModels.StringNodify 
{
    public class CalculatorOperationVM : OperationVM
    {
        public CalculatorVM InnerCalculator { get; } = new  ();

        private OperationVM InnerOutput { get; } = new   OperationVM
        {
            Title = "Output Parameters",
            Input = { new ConnectorVM() },
            Location = new Point(500, 300),
            IsReadOnly = true
        };

        private CalculatorInputOperationVM InnerInput { get; } = new CalculatorInputOperationVM
        {
            Title = "Input Parameters",
            Location = new Point(300, 300),
            IsReadOnly = true
        };

        public CalculatorOperationVM()
        {
            InnerCalculator.Operations.Add(InnerInput);
            InnerCalculator.Operations.Add(InnerOutput);

            Output = new ConnectorVM();

            InnerOutput.Input[0].ValueObservers.Add(Output);

            InnerInput.Output.ForEach(x => Input.Add(new ConnectorVM
            {
                Title = x.Title
            }));

            InnerInput.Output
                .WhenAdded(x => Input.Add(new ConnectorVM
                {
                    Title = x.Title
                }))
                .WhenRemoved(x => Input.RemoveOne(i => i.Title == x.Title));
        }

        protected override void OnInputValueChanged()
        {
            for (var i = 0; i < Input.Count; i++)
            {
                InnerInput.Output[i].Value = Input[i].Value;
            }
        }
    }
}
