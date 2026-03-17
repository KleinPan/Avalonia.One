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

    /// <summary>
    /// 刷新左右文件差异。允许任一侧为空（用于新增/删除文件场景）。
    /// </summary>
    private void RefreshDiff()
    {
        var leftLines = NormalizeLines(LeftContent);
        var rightLines = NormalizeLines(RightContent);

        var result = BuildDiffResult(leftLines, rightLines);

        TotalLineCount = Math.Max(leftLines.Count, rightLines.Count);
        ChangedLineCount = result.ChangedRows;

        OnDiffUpdated?.Invoke(result);
    }

    /// <summary>
    /// 参考 SourceGit 的“按块对齐”思路：
    /// 1) 先用 LCS 得到稳定的 Add/Remove 操作序列；
    /// 2) 在每个变化块内把 Remove/Add 按位置配对为 Modified；
    /// 这样能避免全局配对导致的错误标色。
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

            var modifiedCount = Math.Min(removeLines.Count, addLines.Count);
            for (var i = 0; i < modifiedCount; i++)
            {
                result.LeftChanges.Add(new DiffLine { LineNumber = removeLines[i], Type = DiffType.Modified });
                result.RightChanges.Add(new DiffLine { LineNumber = addLines[i], Type = DiffType.Modified });
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
