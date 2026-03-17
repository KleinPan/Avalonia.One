using Avalonia;
using Avalonia.Media;

using AvaloniaEdit.Rendering;

namespace One.Toolbox.ViewModels.DiffViewer;

public class DiffBackgroundRenderer : IBackgroundRenderer
{
    private readonly List<DiffLine> _lines;

    public DiffBackgroundRenderer(List<DiffLine> lines)
    {
        _lines = lines;
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid)
            return;

        foreach (var line in textView.VisualLines)
        {
            var lineNo = line.FirstDocumentLine.LineNumber;

            var diff = _lines.FirstOrDefault(x => x.LineNumber == lineNo);
            if (diff == null) continue;

            var rect = new Rect(0, line.VisualTop, textView.Bounds.Width, line.Height);

            IBrush brush = diff.Type switch
            {
                DiffType.Added => new SolidColorBrush(Color.Parse("#2233AA33")),
                DiffType.Removed => new SolidColorBrush(Color.Parse("#22FF4444")),
                DiffType.Modified => new SolidColorBrush(Color.Parse("#22CCCC33")),
                _ => null
            };

            if (brush != null)
            {
                drawingContext.FillRectangle(brush, rect);
            }
        }
    }
}