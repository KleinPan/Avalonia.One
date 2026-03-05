using Avalonia.Media;

using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace One.Toolbox.ViewModels.Serialport;

public class LogColorizer : DocumentColorizingTransformer
{
    protected override void ColorizeLine(DocumentLine line)
    {
        string text = CurrentContext.Document.GetText(line);

        // 时间长度固定 12: 15:11:01.511
        if (text.Length >= 12)
        {
            ChangeLinePart(
                line.Offset,
                line.Offset + 12,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(Brushes.Gray);
                });
        }

        // 发送 >>
        int sendIndex = text.IndexOf(">>");
        if (sendIndex >= 0)
        {
            ChangeLinePart(
                line.Offset + sendIndex,
                line.EndOffset,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(Brushes.Green);
                });
        }

        // 接收 <<
        int recvIndex = text.IndexOf("<<");
        if (recvIndex >= 0)
        {
            ChangeLinePart(
                line.Offset + recvIndex,
                line.EndOffset,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(Brushes.Blue);
                });
        }
    }
}