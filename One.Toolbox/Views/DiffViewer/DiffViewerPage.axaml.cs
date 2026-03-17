using Avalonia.Controls;

using AvaloniaEdit.Document;

using One.Toolbox.ViewModels.DiffViewer;

namespace One.Toolbox.Views.DiffViewer;

public partial class DiffViewerPage : UserControl
{
    public DiffViewerPage()
    {
        InitializeComponent(); 
        
        this.AttachedToVisualTree += (_, _) =>
        {
            if (DataContext is DiffViewerPageVM vm)
            {
                vm.OnDiffUpdated = ApplyDiff;
            }
        };
    }
    private void ApplyDiff(DiffResult diff)
    {
        LeftEditor.Document = new TextDocument(diff.LeftText);
        RightEditor.Document = new TextDocument(diff.RightText);

        LeftEditor.TextArea.TextView.BackgroundRenderers.Clear();
        RightEditor.TextArea.TextView.BackgroundRenderers.Clear();

        LeftEditor.TextArea.TextView.BackgroundRenderers.Add(
            new DiffBackgroundRenderer(diff.LeftChanges));

        RightEditor.TextArea.TextView.BackgroundRenderers.Add(
            new DiffBackgroundRenderer(diff.RightChanges));

        LeftEditor.TextArea.TextView.InvalidateLayer(AvaloniaEdit.Rendering.KnownLayer.Selection);
        RightEditor.TextArea.TextView.InvalidateLayer(AvaloniaEdit.Rendering.KnownLayer.Selection);
    }
}
