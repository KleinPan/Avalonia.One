using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.DiffViewer;

public partial class DiffViewerPageVM : BasePageVM
{
    public Action<DiffResult>? OnDiffUpdated;

    [ObservableProperty]
    private string leftFilePath = "将文件拖拽到左侧编辑器";

    [ObservableProperty]
    private string rightFilePath = "将文件拖拽到右侧编辑器";

    [ObservableProperty]
    private string leftContent = string.Empty;

    [ObservableProperty]
    private string rightContent = string.Empty;

    [ObservableProperty]
    private int changedLineCount;

    [ObservableProperty]
    private int totalLineCount;

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
        LeftContent = ReadAllText(path);
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
        RightContent = ReadAllText(path);
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
        if (string.IsNullOrEmpty(LeftContent) || string.IsNullOrEmpty(RightContent))
            return;

        var leftLines = NormalizeLines(LeftContent);
        var rightLines = NormalizeLines(RightContent);

        var max = Math.Max(leftLines.Count, rightLines.Count);

        var result = new DiffResult
        {
            LeftText = LeftContent,
            RightText = RightContent
        };

        for (int i = 0; i < max; i++)
        {
            var left = i < leftLines.Count ? leftLines[i] : null;
            var right = i < rightLines.Count ? rightLines[i] : null;

            if (left == right)
                continue;

            if (left == null)
            {
                result.RightChanges.Add(new DiffLine { LineNumber = i + 1, Type = DiffType.Added });
            }
            else if (right == null)
            {
                result.LeftChanges.Add(new DiffLine { LineNumber = i + 1, Type = DiffType.Removed });
            }
            else
            {
                result.LeftChanges.Add(new DiffLine { LineNumber = i + 1, Type = DiffType.Modified });
                result.RightChanges.Add(new DiffLine { LineNumber = i + 1, Type = DiffType.Modified });
            }
        }

        TotalLineCount = max;
        ChangedLineCount = result.LeftChanges.Count + result.RightChanges.Count;

        OnDiffUpdated?.Invoke(result);
    }

    private static List<string> NormalizeLines(string text)
    {
        return text.Replace("\r\n", "\n").Split('\n').ToList();
    }
}
