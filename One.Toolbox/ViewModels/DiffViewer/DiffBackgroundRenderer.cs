using Avalonia;
using Avalonia.Media;

using AvaloniaEdit.Rendering;

namespace One.Toolbox.ViewModels.DiffViewer;

public class DiffBackgroundRenderer : IBackgroundRenderer
{
    private readonly List<DiffLine> _lines;
    private static readonly IBrush AddedBrush = new SolidColorBrush(Color.Parse("#4433AA33"));
    private static readonly IBrush RemovedBrush = new SolidColorBrush(Color.Parse("#44FF4444"));
    private static readonly IBrush ModifiedBrush = new SolidColorBrush(Color.Parse("#44CCCC33"));

    public DiffBackgroundRenderer(List<DiffLine> lines)
    {
        _lines = lines;
    }

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid)
            return;

        foreach (var line in textView.VisualLines)
        {
            var lineNo = line.FirstDocumentLine.LineNumber;

            var diff = _lines.FirstOrDefault(x => x.LineNumber == lineNo);
            if (diff == null) continue;

            var rect = new Rect(textView.ScrollOffset.X, line.VisualTop, textView.Bounds.Width, line.Height);

            IBrush brush = diff.Type switch
            {
                DiffType.Added => AddedBrush,
                DiffType.Removed => RemovedBrush,
                DiffType.Modified => ModifiedBrush,
                _ => null
            };

            if (brush != null)
            {
                drawingContext.FillRectangle(brush, rect);
            }
        }
    }
}
