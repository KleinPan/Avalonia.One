using Avalonia;
using Avalonia.Media;

using AvaloniaEdit.Rendering;

namespace One.Toolbox.ViewModels.DiffViewer;

public class DiffBackgroundRenderer : IBackgroundRenderer
{
    private readonly Dictionary<int, DiffType> _lineTypes;

    // 颜色参考常见 Git 工具：新增=绿，删除=红，修改=黄。
    private static readonly IBrush AddedBrush = new SolidColorBrush(Color.Parse("#4A2E7D32"));
    private static readonly IBrush RemovedBrush = new SolidColorBrush(Color.Parse("#4AA94442"));
    private static readonly IBrush ModifiedBrush = new SolidColorBrush(Color.Parse("#4A9A8F33"));

    public DiffBackgroundRenderer(IEnumerable<DiffLine> lines)
    {
        _lineTypes = lines
            .GroupBy(x => x.LineNumber)
            .ToDictionary(x => x.Key, x => x.Last().Type);
    }

    public KnownLayer Layer => KnownLayer.Background;

    /// <summary>
    /// 对可见行绘制背景色，避免全量扫描导致滚动卡顿。
    /// </summary>
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid)
        {
            return;
        }

        var viewPortWidth = textView.Bounds.Width;

        foreach (var line in textView.VisualLines)
        {
            var lineNo = line.FirstDocumentLine.LineNumber;
            if (!_lineTypes.TryGetValue(lineNo, out var type))
            {
                continue;
            }

            var rect = new Rect(0, line.VisualTop - textView.ScrollOffset.Y, viewPortWidth, line.Height);
            var brush = type switch
            {
                DiffType.Added => AddedBrush,
                DiffType.Removed => RemovedBrush,
                DiffType.Modified => ModifiedBrush,
                _ => null
            };

            if (brush is not null)
            {
                drawingContext.FillRectangle(brush, rect);
            }
        }
    }
}
