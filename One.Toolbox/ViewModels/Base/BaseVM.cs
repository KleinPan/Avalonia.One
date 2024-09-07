using One.SimpleLog;
using One.SimpleLog.Extensions;
using One.SimpleLog.Loggers;

using System.Threading;

namespace One.Toolbox.ViewModels.Base;

public class BaseVM : ObservableObject
{
    public static LoggerWrapper logger = LogManager.GetLogger();
    protected bool isInitialized = false;

    /// <summary> 进入当前页面 </summary>
    public virtual void OnNavigatedEnter()
    {
        if (!isInitialized)
            InitializeViewModel();
    }

    /// <summary> 从当前页面离开 </summary>
    public virtual void OnNavigatedLeave() { }

   

    public virtual void InitializeViewModel()
    {
        isInitialized = true;

        WriteDebugLog($"{this.ToString()}");
    }

  
    public virtual void WriteDebugLog(string msg)
    {
        logger.WithPatternProperty("ThreadID", Thread.CurrentThread.ManagedThreadId.ToString()).Debug(msg);
    }

    public virtual void WriteInfoLog(string msg)
    {
        logger.Info(msg);

    }
}
