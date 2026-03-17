using Avalonia.Media;
using One.Toolbox.ViewModels.Base;
using System.Collections.ObjectModel;

namespace One.Toolbox.ViewModels.DiffViewer;

public partial class DiffViewerPageVM : BasePageVM
{
    [ObservableProperty]
    private string leftFilePath = "将文件拖拽到左侧区域";

    [ObservableProperty]
    private string rightFilePath = "将文件拖拽到右侧区域";

    [ObservableProperty]
    private int changedLineCount;

    [ObservableProperty]
    private int totalLineCount;

    [ObservableProperty]
    private ObservableCollection<DiffLineItemVM> diffLines = [];

    private string? _leftContent;
    private string? _rightContent;

    public override void UpdateTitle()
    {
        Title = "Diff Viewer";
    }

    [RelayCommand]
    private void DropLeft(object? obj)
    {
        var path = GetFirstFilePath(obj);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        LeftFilePath = path;
        _leftContent = ReadAllText(path);
        RefreshDiff();
    }

    [RelayCommand]
    private void DropRight(object? obj)
    {
        var path = GetFirstFilePath(obj);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        RightFilePath = path;
        _rightContent = ReadAllText(path);
        RefreshDiff();
    }

    private static string? GetFirstFilePath(object? obj)
    {
        if (obj is not List<Uri> files || files.Count == 0)
        {
            return null;
        }

        var uri = files[0];
        return uri.IsFile ? uri.LocalPath : uri.ToString();
    }

    private static string ReadAllText(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch
        {
            return string.Empty;
        }
    }

    private void RefreshDiff()
    {
        if (_leftContent is null || _rightContent is null)
        {
            return;
        }

        var leftLines = NormalizeLines(_leftContent);
        var rightLines = NormalizeLines(_rightContent);

        var max = Math.Max(leftLines.Count, rightLines.Count);
        var changed = 0;
        var items = new List<DiffLineItemVM>(max);

        for (var i = 0; i < max; i++)
        {
            var left = i < leftLines.Count ? leftLines[i] : string.Empty;
            var right = i < rightLines.Count ? rightLines[i] : string.Empty;
            var isChanged = !string.Equals(left, right, StringComparison.Ordinal);

            if (isChanged)
            {
                changed++;
            }

            items.Add(new DiffLineItemVM
            {
                LineNumber = i + 1,
                LeftText = left,
                RightText = right,
                RowBackground = isChanged ? new SolidColorBrush(Color.Parse("#25FFB3B3")) : Brushes.Transparent,
            });
        }

        TotalLineCount = max;
        ChangedLineCount = changed;
        DiffLines = new ObservableCollection<DiffLineItemVM>(items);
    }

    private static List<string> NormalizeLines(string text)
    {
        return text.Replace("\r\n", "\n").Split('\n').ToList();
    }
}

public partial class DiffLineItemVM : ObservableObject
{
    [ObservableProperty]
    private int lineNumber;

    [ObservableProperty]
    private string leftText = string.Empty;

    [ObservableProperty]
    private string rightText = string.Empty;

    [ObservableProperty]
    private IBrush rowBackground = Brushes.Transparent;
}
