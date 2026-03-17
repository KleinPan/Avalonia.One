using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public string LeftText { get; set; } = "";
    public string RightText { get; set; } = "";
    public List<DiffLine> LeftChanges { get; set; } = new();
    public List<DiffLine> RightChanges { get; set; } = new();
}