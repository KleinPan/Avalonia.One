using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

using One.Toolbox.ViewModels.DiffViewer;

namespace One.Toolbox.Views.DiffViewer;

public partial class DiffViewerPage : UserControl
{
    private ScrollViewer? _leftScroller;
    private ScrollViewer? _rightScroller;
    private bool _isSyncingScroll;

    public DiffViewerPage()
    {
        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is not DiffViewerPageVM vm)
        {
            return;
        }

        vm.OnDiffUpdated = ApplyDiff;
        vm.OnNavigateToDiffLine = ScrollToDiffLine;
        vm.PropertyChanged += OnViewModelPropertyChanged;

        AttachScrollSync();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is DiffViewerPageVM vm)
        {
            vm.PropertyChanged -= OnViewModelPropertyChanged;
            vm.OnNavigateToDiffLine = null;
            vm.OnDiffUpdated = null;
        }

        DetachScrollSync();
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

        AttachScrollSync();
    }

    /// <summary>
    /// 跳转到指定差异行。
    /// </summary>
    private void ScrollToDiffLine(int line)
    {
        if (line <= 0)
        {
            return;
        }

        LeftEditor.ScrollToLine(line);
        RightEditor.ScrollToLine(line);
    }

    /// <summary>
    /// 绑定左右编辑器滚动条，实现可开关的同步滚动。
    /// </summary>
    private void AttachScrollSync()
    {
        if (_leftScroller != null && _rightScroller != null)
        {
            return;
        }

        _leftScroller = FindScrollViewer(LeftEditor);
        _rightScroller = FindScrollViewer(RightEditor);

        if (_leftScroller is null || _rightScroller is null)
        {
            return;
        }

        _leftScroller.ScrollChanged += OnLeftScrollChanged;
        _rightScroller.ScrollChanged += OnRightScrollChanged;
    }

    private void DetachScrollSync()
    {
        if (_leftScroller is not null)
        {
            _leftScroller.ScrollChanged -= OnLeftScrollChanged;
        }

        if (_rightScroller is not null)
        {
            _rightScroller.ScrollChanged -= OnRightScrollChanged;
        }

        _leftScroller = null;
        _rightScroller = null;
    }

    private static ScrollViewer? FindScrollViewer(TextEditor editor)
    {
        return editor
            .GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
    }

    private void OnLeftScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        SyncScroll(_leftScroller, _rightScroller);
    }

    private void OnRightScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        SyncScroll(_rightScroller, _leftScroller);
    }

    private void SyncScroll(ScrollViewer? from, ScrollViewer? to)
    {
        if (_isSyncingScroll || from is null || to is null || DataContext is not DiffViewerPageVM vm || !vm.IsSyncScrollEnabled)
        {
            return;
        }

        _isSyncingScroll = true;
        to.Offset = new Vector(to.Offset.X, from.Offset.Y);
        _isSyncingScroll = false;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DiffViewerPageVM.IsSyncScrollEnabled) || DataContext is not DiffViewerPageVM vm)
        {
            return;
        }

        if (vm.IsSyncScrollEnabled)
        {
            ResetScrollToDefault();
        }
    }

    /// <summary>
    /// 启用同步滚动时，统一重置到顶部默认位置。
    /// </summary>
    private void ResetScrollToDefault()
    {
        if (_leftScroller is null || _rightScroller is null)
        {
            return;
        }

        _leftScroller.Offset = new Vector(_leftScroller.Offset.X, 0);
        _rightScroller.Offset = new Vector(_rightScroller.Offset.X, 0);

        LeftEditor.ScrollToLine(1);
        RightEditor.ScrollToLine(1);
    }

    private static void ReplaceDiffRenderer(TextEditor editor, IReadOnlyList<DiffLine> lines)
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
