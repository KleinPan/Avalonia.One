using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.ViewModels.Base;

namespace One.Toolbox.ViewModels.DiffViewer;

public partial class DiffViewerPageVM : BasePageVM
{
    private enum DiffOpType
    {
        Equal,
        Add,
        Remove
    }

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
    /// 参考 SourceGit 的按块对齐思路：
    /// 在同一变化块内配对 Remove/Add 为 Modified，并记录差异块锚点。
    /// </summary>
    private static DiffResult BuildDiffResult(IReadOnlyList<string> leftLines, IReadOnlyList<string> rightLines)
    {
        var result = new DiffResult
        {
            LeftText = string.Join(Environment.NewLine, leftLines),
            RightText = string.Join(Environment.NewLine, rightLines)
        };

        var ops = BuildDiffOps(leftLines, rightLines);
        var changedRows = 0;

        var index = 0;
        var leftLine = 1;
        var rightLine = 1;
        while (index < ops.Count)
        {
            if (ops[index] == DiffOpType.Equal)
            {
                leftLine++;
                rightLine++;
                index++;
                continue;
            }

            var removeLines = new List<int>();
            var addLines = new List<int>();

            while (index < ops.Count && ops[index] != DiffOpType.Equal)
            {
                if (ops[index] == DiffOpType.Remove)
                {
                    removeLines.Add(leftLine++);
                }
                else
                {
                    addLines.Add(rightLine++);
                }

                index++;
            }

            var anchor = removeLines.FirstOrDefault();
            if (anchor <= 0)
            {
                anchor = addLines.FirstOrDefault();
            }

            if (anchor > 0)
            {
                result.AnchorLines.Add(anchor);
            }

            var modifiedCount = Math.Min(removeLines.Count, addLines.Count);
            for (var i = 0; i < modifiedCount; i++)
            {
                // 与 SourceGit 一致：左侧修改行按删除（红色）显示，右侧按新增（绿色）显示。
                result.LeftChanges.Add(new DiffLine { LineNumber = removeLines[i], Type = DiffType.Removed });
                result.RightChanges.Add(new DiffLine { LineNumber = addLines[i], Type = DiffType.Added });
            }

            for (var i = modifiedCount; i < removeLines.Count; i++)
            {
                result.LeftChanges.Add(new DiffLine { LineNumber = removeLines[i], Type = DiffType.Removed });
            }

            for (var i = modifiedCount; i < addLines.Count; i++)
            {
                result.RightChanges.Add(new DiffLine { LineNumber = addLines[i], Type = DiffType.Added });
            }

            changedRows += Math.Max(removeLines.Count, addLines.Count);
        }

        result.ChangedRows = changedRows;
        return result;
    }

    /// <summary>
    /// 通过 LCS 回溯构建操作序列（Equal/Add/Remove）。
    /// </summary>
    private static List<DiffOpType> BuildDiffOps(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        var lcs = BuildLcsMatrix(left, right);
        var ops = new List<DiffOpType>();

        var i = 0;
        var j = 0;
        while (i < left.Count && j < right.Count)
        {
            if (left[i] == right[j])
            {
                ops.Add(DiffOpType.Equal);
                i++;
                j++;
                continue;
            }

            if (lcs[i + 1, j] >= lcs[i, j + 1])
            {
                ops.Add(DiffOpType.Remove);
                i++;
            }
            else
            {
                ops.Add(DiffOpType.Add);
                j++;
            }
        }

        while (i++ < left.Count)
        {
            ops.Add(DiffOpType.Remove);
        }

        while (j++ < right.Count)
        {
            ops.Add(DiffOpType.Add);
        }

        return ops;
    }

    /// <summary>
    /// 构建 LCS 长度矩阵：用于稳定决定“当前是 Add 还是 Remove”。
    /// </summary>
    private static int[,] BuildLcsMatrix(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        var m = left.Count;
        var n = right.Count;
        var matrix = new int[m + 1, n + 1];

        for (var i = m - 1; i >= 0; i--)
        {
            for (var j = n - 1; j >= 0; j--)
            {
                matrix[i, j] = left[i] == right[j]
                    ? matrix[i + 1, j + 1] + 1
                    : Math.Max(matrix[i + 1, j], matrix[i, j + 1]);
            }
        }

        return matrix;
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

