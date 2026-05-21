using Avalonia.Controls;

using One.Toolbox.ViewModels.Serialport;

namespace One.Toolbox.Views.NetTool;

public partial class NetToolPage : UserControl
{
    public NetToolPage()
    {
        InitializeComponent();
        LogEditor.TextArea.TextView.LineTransformers.Add(new LogColorizer());
    }
}