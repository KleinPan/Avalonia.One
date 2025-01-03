using Microsoft.Extensions.DependencyInjection;

using One.Base.Helpers;
using One.Control.Markup.I18n;
using One.Toolbox.Assets.Languages;
using One.Toolbox.ExtensionMethods;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using Vanara.Extensions;

using static Vanara.PInvoke.RstrtMgr;

using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace One.Toolbox.ViewModels.FileMonitor;

public partial class FileMonitorPageVM : BasePageVM
{
    public FileMonitorPageVM()
    {
    }

    public override void UpdateTitle()
    {
        Title = I18nManager.GetString(Language.FileMonitor);
    }



    [ObservableProperty]
    private string filePath;

    public ObservableCollection<FileInUseVM> ProcessList { get; set; } = new ObservableCollection<FileInUseVM>();

    [RelayCommand]
    private void Drop(object obj)
    {
        List<Uri>? Last = obj as List<Uri>;
        Uri? tem = Last?.Last();

        FilePath = tem.LocalPath;

        TestFile(FilePath);
    }

    private void TestFile(string filePath)
    {
        ProcessList.Clear();

        StringBuilder stringBuilder = new StringBuilder();
        RmStartSession(out uint pSessionHandel, 0, stringBuilder).Judge();

        RmRegisterResources(pSessionHandel, 1, [filePath], 0, null, 0, null).Judge();

        uint nProcInfo = 10;
        RM_PROCESS_INFO[] rM_PROCESS_INFOs = new RM_PROCESS_INFO[nProcInfo];
        RmGetList(pSessionHandel, out uint nProcInfoNeeded, ref nProcInfo, rM_PROCESS_INFOs, out RM_REBOOT_REASON lpdwRebootReasons).Judge();

        if (nProcInfoNeeded == 0)
        {
            ServiceHelper.Instance.ShowInfoMessage("This file is not in locked!");

            return;
        }

        foreach (RM_PROCESS_INFO item in rM_PROCESS_INFOs)
        {
            if (item.Process.dwProcessId == 0)
            {
                continue;
            }
            FileInUseVM fIleInUseVM = new FileInUseVM();
            fIleInUseVM.LockFileName = item.strAppName;
            fIleInUseVM.LockProcessID = item.Process.dwProcessId;

            fIleInUseVM.ProcessStartTime = item.Process.ProcessStartTime.ToDateTime();
            fIleInUseVM.RefreshAction += RefreshAction;

            ProcessList.Add(fIleInUseVM);
        }

        RmEndSession(pSessionHandel);
    }

    private void RefreshAction(FileInUseVM vm)
    {
        ProcessList.Remove(vm);
    }
}

public partial class FileInUseVM : BaseVM
{
    [ObservableProperty]
    private string lockFileName;

    [ObservableProperty]
    private uint lockProcessID;

    [ObservableProperty]
    private DateTime processStartTime;

    public Action<FileInUseVM> RefreshAction;

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetProcessTimes(IntPtr hProcess, out FILETIME lpCreationTime, out FILETIME lpExitTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

    [RelayCommand]
    private void KillProcess()
    {
        //判断平台
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        try
        {
            var thisproc = Process.GetProcessById((int)LockProcessID);

            bool res = GetProcessTimes(thisproc.Handle, out FILETIME lpCreationTime, out FILETIME lpExitTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);
            if (res)
            {
                if (lpCreationTime.ToDateTime() == ProcessStartTime)
                {
                    ProcessHelper.KillProcessByID((int)LockProcessID);
                    RefreshAction?.Invoke(this);
                }
            }
        }
        catch (Exception ex)
        {
            App.Current!.Services.GetService<INotifyService>()!.ShowErrorMessage(ex.ToString());
        }
    }
}