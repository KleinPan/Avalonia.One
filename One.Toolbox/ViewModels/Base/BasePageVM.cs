using Avalonia.Controls;

using CommunityToolkit.Mvvm.Messaging;

using One.SimpleLog;
using One.SimpleLog.Extensions;
using One.SimpleLog.Loggers;

namespace One.Toolbox.ViewModels.Base;

public partial class BasePageVM : BaseVM
{
    public static LoggerWrapper logger = LogManager.GetLogger();
    protected bool isInitialized = false;

    [ObservableProperty]
    private string title;

    protected UserControl currentPage;

    /// <summary>进入当前页面</summary>
    public virtual void OnNavigatedEnter(UserControl userControl)
    {
        if (!isInitialized)
        {
            currentPage = userControl;
            InitializeViewModel();

            //Task.Run(() =>
            //{
            //    Task.Delay(10);
            //    currentPage = userControl;
            //});
        }
    }

    /// <summary>从当前页面离开</summary>
    public virtual void OnNavigatedLeave() { }

    public virtual void InitializeViewModel()
    {
        isInitialized = true;

        WriteDebugLog($"{this.ToString()}");

        UpdateTitle();
        //https://learn.microsoft.com/zh-cn/dotnet/communitytoolkit/mvvm/messenger

        //语言更新消息
        WeakReferenceMessenger.Default.Register<string>(this, (r, m) =>
        {
            // Handle the message here, with r being the recipient and m being the input message.
            // Using the recipient passed as input makes it so that the lambda expression doesn't
            // capture "this", improving performance.

            UpdateTitle();
        });
    }

    public virtual void WriteDebugLog(string msg)
    {
        logger.WithPatternProperty("ThreadID", Thread.CurrentThread.ManagedThreadId.ToString()).Debug(msg);
    }

    public virtual void WriteInfoLog(string msg)
    {
        logger.Info(msg);
    }
    public virtual void WriteWarnLog(string msg)
    {
        logger.Warn(msg);
    }
    public virtual void WriteErrorLog(string msg)
    {
        logger.Error(msg);
    }
    public virtual void UpdateTitle()
    {
    }
}