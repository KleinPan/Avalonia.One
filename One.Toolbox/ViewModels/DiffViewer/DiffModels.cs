namespace One.Toolbox.ViewModels.DiffViewer;

public enum DiffType
{
    Unchanged,
    Added,
    Removed,
    Modified
}

public class DiffLine
{
    public int LineNumber { get; set; }
    public DiffType Type { get; set; }
}

public class DiffResult
{
    public string LeftText { get; set; } = string.Empty;
    public string RightText { get; set; } = string.Empty;
    public List<DiffLine> LeftChanges { get; set; } = new();
    public List<DiffLine> RightChanges { get; set; } = new();
    public int ChangedRows { get; set; }
}
