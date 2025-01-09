using Avalonia.Controls;
using Avalonia.Threading;

using Microsoft.Extensions.DependencyInjection;

using One.Control.Helpers;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace One.Toolbox.ViewModels.RegularTester;

public partial class RegularTesterPageVM : BasePageVM
{
    [ObservableProperty]
    private string inputText = "This is a a farm that that raises dairy cattle.";

    [ObservableProperty]
    private string pattern = @"\b(\w+)\W+(\1)\b";

    /// <summary>使用不区分大小写的匹配。</summary>
    [ObservableProperty]
    private bool ignoreCase;

    /// <summary>从模式中排除保留的空白并启用数字符号 (#) 后的注释。</summary>
    [ObservableProperty]
    private bool ignorePatternWhitespace;

    /// <summary> 不捕获未命名的组。 唯一有效的捕获是显式命名或编号的 (?<name> subexpression) 形式的组。 </summary>
    [ObservableProperty]
    private bool explicitCapture;

    /// <summary>使用多线模式，其中 ^ 和 $ 表示每行的开头和末尾（不是输入字符串的开头和末尾）。</summary>
    [ObservableProperty]
    private bool multiline = true;

    /// <summary>使用单行模式，其中的句号 (.) 匹配每个字符（而不是除了 \n 以外的每个字符)。</summary>
    [ObservableProperty]
    private bool singleline;

    /// <summary>更改搜索方向。 搜索是从右向左而不是从左向右进行。</summary>
    [ObservableProperty]
    private bool rightToLeft;

    private RegexOptions _regexOptions;

    public ObservableCollection<Nodes> MathReslut { get; set; } = new();

    [ObservableProperty]
    private Nodes currentNode;

    partial void OnCurrentNodeChanged(Nodes value)
    {
        if (value == null)
        {
            return;
        }

        //Dispatcher.UIThread.Post(() =>
        //{
        //    var a = TopLevel.GetTopLevel(currentPage);

        //    var textBox = MyVisualTreeHelper.FindControlByName<TextBox>(a, "txbInput");
        //    textBox.SelectionStart = value.IndexInText;
        //    textBox.SelectionEnd = value.EndInText;
        //});
    }

    [ObservableProperty]
    private string outputText;

    #region Combox

    [ObservableProperty]
    private ObservableCollection<PatternVM> prePattern = new();

    [ObservableProperty]
    private PatternVM currentPrePattern;

    partial void OnCurrentPrePatternChanged(PatternVM value)
    {
        Pattern = value.Pattern;
    }

    #endregion Combox

    public RegularTesterPageVM()
    {
        PrePattern.Add(new PatternVM()
        {
            Description = "Email地址",
            Pattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
        });
        PrePattern.Add(new PatternVM()
        {
            Description = "手机号码",
            Pattern = @"1[345678]\d{9}$"
        });

        PrePattern.Add(new PatternVM()
        {
            Description = "中文字符",
            Pattern = @"[\u4e00-\u9fa5]"
        });
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.RegTestTool);
    }

    public override void InitializeViewModel()
    {
        base.InitializeViewModel();

        try
        {
            if (IgnorePatternWhitespace)
            {
                _regexOptions = _regexOptions | RegexOptions.IgnorePatternWhitespace;
            }
            if (IgnoreCase)
            {
                _regexOptions = _regexOptions | RegexOptions.IgnoreCase;
            }
            if (Multiline)
            {
                _regexOptions = _regexOptions | RegexOptions.Multiline;
            }
            if (Singleline)
            {
                _regexOptions = _regexOptions | RegexOptions.Singleline;
            }
            if (RightToLeft)
            {
                _regexOptions = _regexOptions | RegexOptions.RightToLeft;
            }
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.ToString());
        }
    }

    [RelayCommand]
    void StartTest()
    {
        CheckInput();
        CheckPattern();

        try
        {
            MathReslut.Clear();
            var regex = GenerateRegex(Pattern);
            var match = regex.Match(InputText);
            int i = 1;
            while (match.Success)
            {
                if (match.Value.Length > 0)
                {
                    Nodes matchResult = new Nodes();
                    matchResult.Description = "匹配记录";
                    matchResult.Index = i;
                    matchResult.Text = match.Value;
                    matchResult.IndexInText = match.Index;
                    matchResult.Length = match.Length;

                    for (int j = 1; j < match.Groups.Count; j++)
                    {
                        Nodes subNodes = new Nodes();
                        subNodes.Description = "匹配组";
                        subNodes.Index = j;
                        subNodes.Text = match.Groups[j].Value;
                        subNodes.IndexInText = match.Groups[j].Index;
                        subNodes.Length = match.Groups[j].Length;

                        matchResult.SubNodes.Add(subNodes);
                    }
                    MathReslut.Add(matchResult);
                }

                i++;
                match = match.NextMatch();
            }
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.ToString());
        }
    }

    private Regex GenerateRegex(string pattern)
    {
        return new Regex(pattern, _regexOptions);
    }

    void CheckPattern()
    {
        if (string.IsNullOrEmpty(Pattern))
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage("未指定正则表达式，请输入正确的正则表达式。");

            return;
        }
    }

    void CheckInput()
    {
        if (string.IsNullOrEmpty(InputText))
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage("未指定输入文本，请在输入框内指定有效的文本内容。");

            return;
        }
    }
}

public partial class Nodes : BaseVM
{
    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private string text;

    [NotifyPropertyChangedFor(nameof(EndInText))]
    [ObservableProperty]
    private int indexInText;

    [NotifyPropertyChangedFor(nameof(EndInText))]
    [ObservableProperty]
    private int length;

    public int EndInText => IndexInText + Length;

    public ObservableCollection<Nodes> SubNodes { get; set; } = new();
}