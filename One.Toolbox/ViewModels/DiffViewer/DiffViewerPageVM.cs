using DiffPlex;

using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Services.Navigation;
using One.Toolbox.ViewModels.Base;
using One.Toolbox.Views.DiffViewer;

namespace One.Toolbox.ViewModels.DiffViewer;

[ToolPage(60, Language.DiffViewer, "text_number_format_regular", typeof(DiffViewerPage))]
public partial class DiffViewerPageVM : BasePageVM
{
    private readonly List<int> _anchors = new();
    private int _refreshVersion;

    public Action<DiffResult>? OnDiffUpdated;
    public Action<int>? OnNavigateToDiffLine;

    [ObservableProperty]
    private string leftFilePath = I18nManager.GetString(Language.DiffDropLeftHint)!;

    [ObservableProperty]
    private string rightFilePath = I18nManager.GetString(Language.DiffDropRightHint)!;

    [ObservableProperty]
    private string leftContent = string.Empty;

    [ObservableProperty]
    private string rightContent = string.Empty;

    [ObservableProperty]
    private int changedLineCount;

    [ObservableProperty]
    private int totalLineCount;

    [ObservableProperty]
    private bool isSyncScrollEnabled = true;

    [ObservableProperty]
    private int currentDiffIndex = -1;


    public string DiffPositionText => _anchors.Count == 0 ? "0/0" : $"{CurrentDiffIndex + 1}/{_anchors.Count}";

    partial void OnCurrentDiffIndexChanged(int value)
    {
        OnPropertyChanged(nameof(DiffPositionText));
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.DiffViewer)!;     
        LeftFilePath = I18nManager.GetString(Language.DiffDropLeftHint)!;      
        RightFilePath = I18nManager.GetString(Language.DiffDropRightHint)!;
    }

    [RelayCommand]
    private async Task DropLeft(object? obj)
    {
        var path = GetFirstFilePath(obj);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        LeftFilePath = path;
        LeftContent = await ReadAllTextAsync(path);
        await RefreshDiffAsync();
    }

    [RelayCommand]
    private async Task DropRight(object? obj)
    {
        var path = GetFirstFilePath(obj);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        RightFilePath = path;
        RightContent = await ReadAllTextAsync(path);
        await RefreshDiffAsync();
    }

    /// <summary>
    /// 跳到首个差异块。
    /// </summary>
    [RelayCommand]
    private void FirstDiff()
    {
        if (_anchors.Count == 0)
        {
            return;
        }

        CurrentDiffIndex = 0;
        OnNavigateToDiffLine?.Invoke(_anchors[CurrentDiffIndex]);
    }

    /// <summary>
    /// 跳到上一个差异块。
    /// </summary>
    [RelayCommand]
    private void PreviousDiff()
    {
        if (_anchors.Count == 0)
        {
            return;
        }

        CurrentDiffIndex = CurrentDiffIndex <= 0 ? 0 : CurrentDiffIndex - 1;
        OnNavigateToDiffLine?.Invoke(_anchors[CurrentDiffIndex]);
    }

    /// <summary>
    /// 跳到下一个差异块。
    /// </summary>
    [RelayCommand]
    private void NextDiff()
    {
        if (_anchors.Count == 0)
        {
            return;
        }

        CurrentDiffIndex = CurrentDiffIndex < 0 ? 0 : Math.Min(CurrentDiffIndex + 1, _anchors.Count - 1);
        OnNavigateToDiffLine?.Invoke(_anchors[CurrentDiffIndex]);
    }

    /// <summary>
    /// 跳到最后一个差异块。
    /// </summary>
    [RelayCommand]
    private void LastDiff()
    {
        if (_anchors.Count == 0)
        {
            return;
        }

        CurrentDiffIndex = _anchors.Count - 1;
        OnNavigateToDiffLine?.Invoke(_anchors[CurrentDiffIndex]);
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

    private static async Task<string> ReadAllTextAsync(string path)
    {
        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 刷新左右文件差异，并同步导航锚点。
    /// </summary>
    private async Task RefreshDiffAsync()
    {
        var leftContentSnapshot = LeftContent;
        var rightContentSnapshot = RightContent;
        var refreshVersion = Interlocked.Increment(ref _refreshVersion);

        var (leftLinesCount, rightLinesCount, result) = await Task.Run(() =>
        {
            var leftLines = NormalizeLines(leftContentSnapshot);
            var rightLines = NormalizeLines(rightContentSnapshot);
            var diffResult = BuildDiffResult(leftLines, rightLines);
            return (leftLines.Count, rightLines.Count, diffResult);
        });

        if (refreshVersion != _refreshVersion)
        {
            return;
        }

        TotalLineCount = Math.Max(leftLinesCount, rightLinesCount);
        ChangedLineCount = result.ChangedRows;

        _anchors.Clear();
        _anchors.AddRange(result.AnchorLines);
        CurrentDiffIndex = _anchors.Count > 0 ? 0 : -1;
        OnPropertyChanged(nameof(DiffPositionText));

        OnDiffUpdated?.Invoke(result);
    }

    /// <summary>
    /// 用 DiffPlex 计算行级差异，并按块对齐：同块内 Remove/Add 配对为修改，记录锚点。
    /// </summary>
    private static DiffResult BuildDiffResult(IReadOnlyList<string> leftLines, IReadOnlyList<string> rightLines)
    {
        var result = new DiffResult
        {
            LeftText = string.Join(Environment.NewLine, leftLines),
            RightText = string.Join(Environment.NewLine, rightLines)
        };

        var diff = Differ.Instance.CreateLineDiffs(
            string.Join('\n', leftLines),
            string.Join('\n', rightLines),
            ignoreWhitespace: false);

        var changedRows = 0;

        foreach (var block in diff.DiffBlocks)
        {
            var anchor = block.DeleteCountA > 0
                ? block.DeleteStartA + 1
                : block.InsertStartB + 1;
            result.AnchorLines.Add(anchor);

            // 与 SourceGit 一致：左侧修改行按删除（红色）显示，右侧按新增（绿色）显示。
            for (var i = 0; i < block.DeleteCountA; i++)
            {
                result.LeftChanges.Add(new DiffLine
                {
                    LineNumber = block.DeleteStartA + 1 + i,
                    Type = DiffType.Removed
                });
            }

            for (var i = 0; i < block.InsertCountB; i++)
            {
                result.RightChanges.Add(new DiffLine
                {
                    LineNumber = block.InsertStartB + 1 + i,
                    Type = DiffType.Added
                });
            }

            changedRows += Math.Max(block.DeleteCountA, block.InsertCountB);
        }

        result.ChangedRows = changedRows;
        return result;
    }

    private static List<string> NormalizeLines(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<string>();
        }

        var normalized = text.Replace("\r\n", "\n");
        if (normalized.EndsWith('\n'))
        {
            normalized = normalized[..^1];
        }

        return string.IsNullOrEmpty(normalized)
            ? new List<string>()
            : normalized.Split('\n').ToList();
    }
}

