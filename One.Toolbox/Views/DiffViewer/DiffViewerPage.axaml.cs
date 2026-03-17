using Avalonia.Controls;

using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

using One.Toolbox.ViewModels.DiffViewer;

namespace One.Toolbox.Views.DiffViewer;

public partial class DiffViewerPage : UserControl
{
    public DiffViewerPage()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) =>
        {
            if (DataContext is DiffViewerPageVM vm)
            {
                vm.OnDiffUpdated = ApplyDiff;
            }
        };
    }

    /// <summary>
    /// 更新文本和差异高亮，仅替换 DiffBackgroundRenderer，避免影响编辑器默认渲染器。
    /// </summary>
    private void ApplyDiff(DiffResult diff)
    {
        LeftEditor.Document = new TextDocument(diff.LeftText);
        RightEditor.Document = new TextDocument(diff.RightText);

        ReplaceDiffRenderer(LeftEditor, diff.LeftChanges);
        ReplaceDiffRenderer(RightEditor, diff.RightChanges);

        LeftEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        RightEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    }

    private static void ReplaceDiffRenderer(AvaloniaEdit.TextEditor editor, IReadOnlyList<DiffLine> lines)
    {
        var renderers = editor.TextArea.TextView.BackgroundRenderers;
        for (var i = renderers.Count - 1; i >= 0; i--)
        {
            if (renderers[i] is DiffBackgroundRenderer)
            {
                renderers.RemoveAt(i);
            }
        }

        renderers.Add(new DiffBackgroundRenderer(lines));
    }
}
