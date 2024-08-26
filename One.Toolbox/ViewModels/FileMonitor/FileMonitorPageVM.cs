using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using One.Base.Helpers;
using One.Toolbox.ExtensionMethods;
using One.Toolbox.Services;
using One.Toolbox.ViewModels.Base;
using Vanara.Extensions;
using static Vanara.PInvoke.RstrtMgr;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace One.Toolbox.ViewModels.FileMonitor;

public partial class FileMonitorPageVM : BaseVM
{
    public FileMonitorPageVM() { }

    public override void OnNavigatedEnter()
    {
        base.OnNavigatedEnter();
        InitData();
    }

    void InitData() { }

    [ObservableProperty]
    private string filePath;

    public ObservableCollection<FIleInUseVM> ProcessList { get; set; } = new ObservableCollection<FIleInUseVM>();

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
            FIleInUseVM fIleInUseVM = new FIleInUseVM();
            fIleInUseVM.LockFileName = item.strAppName;
            fIleInUseVM.LockProcessID = item.Process.dwProcessId;

            fIleInUseVM.ProcessStartTime = item.Process.ProcessStartTime.ToDateTime();
            fIleInUseVM.RefreshAction += RefreshAction;

            ProcessList.Add(fIleInUseVM);
        }

        RmEndSession(pSessionHandel);

        
    }
    private void RefreshAction(FIleInUseVM vm)
    {
        ProcessList.Remove(vm);
    }
}

public partial class FIleInUseVM : BaseVM
{
    [ObservableProperty]
    private string lockFileName;

    [ObservableProperty]
    private uint lockProcessID;

    [ObservableProperty]
    private DateTime processStartTime;


    public Action<FIleInUseVM> RefreshAction;

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
            WriteInfoLog(ex.ToString());
        }
    }
}
