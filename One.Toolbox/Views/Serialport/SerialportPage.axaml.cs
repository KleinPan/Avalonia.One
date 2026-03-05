using Avalonia.Controls;

using One.Toolbox.ViewModels.Serialport;

namespace One.Toolbox.Views;

public partial class SerialportPage : UserControl
{
    public SerialportPage()
    {
        InitializeComponent();
        LogEditor.TextArea.TextView.LineTransformers.Add(new LogColorizer());
    }
}