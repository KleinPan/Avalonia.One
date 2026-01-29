using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace One.Base.Helpers;

/// <summary>进程执行辅助类，提供运行外部程序的功能</summary>
public class ProcessHelper : BaseHelper, IDisposable
{
    private Process? _process;
    private bool _disposed = false;

    public ProcessHelper(Action<string>? logAction = null) : base(logAction)
    {
    }

    /// <summary>配置进程启动信息</summary>
    private void ConfigureProcessStartInfo(ProcessStartInfo startInfo, string exeWorkDirectory, string exeName, string parameters)
    {
        startInfo.WorkingDirectory = exeWorkDirectory;
        startInfo.FileName = System.IO.Path.Combine(exeWorkDirectory, exeName);
        startInfo.Arguments = parameters;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;
    }

    /// <summary>获取或创建进程对象</summary>
    private Process GetOrCreateProcess()
    {
        if (_process != null)
        {
            _process.Dispose();
        }
        return _process = new Process();
    }

    /// <summary>运行可执行程序（异步）</summary>
    public void RunExe(string exeWorkDirectory, string exeName, string parameters)
    {
        try
        {
            using (var process = new Process())
            {
                WriteLog($"ExecuteDir: {exeWorkDirectory}{exeName}, Parameters: {parameters}");
                ConfigureProcessStartInfo(process.StartInfo, exeWorkDirectory, exeName, parameters);
                process.Start();
                process.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            WriteLog($"Error executing process: {ex.Message}");
        }
    }

    /// <summary>运行可执行程序并读取结果</summary>
    public bool RunExeAndReadResult(string exeWorkDirectory, string exeName, string parameters, out string result)
    {
        result = string.Empty;
        try
        {
            using (var process = new Process())
            {
                WriteLog($"ExecuteDir: {exeWorkDirectory}{exeName}, Parameters: {parameters}");
                ConfigureProcessStartInfo(process.StartInfo, exeWorkDirectory, exeName, parameters);
                process.Start();

                try
                {
                    while (!process.HasExited)
                    {
                        var line = process.StandardOutput.ReadLine();
                        if (line != null)
                        {
                            WriteLog($"Output: {line}");
                            result += line;
                        }
                    }

                    var remaining = process.StandardOutput.ReadToEnd();
                    result += remaining;
                    if (!string.IsNullOrEmpty(remaining))
                        WriteLog($"ReadToEnd: {remaining}");

                    var errors = process.StandardError.ReadToEnd();
                    result += errors;
                    if (!string.IsNullOrEmpty(errors))
                        WriteLog($"StandardError: {errors}");

                    process.WaitForExit(1000);
                    WriteLog($"ExitCode: {process.ExitCode}");
                    return process.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    WriteLog($"Error reading process output: {ex.Message}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            WriteLog($"Error executing process: {ex.Message}");
            return false;
        }
    }

    /// <summary>运行可执行程序并读取结果，指定时间限制（超时则杀死进程）</summary>
    /// <param name="exeWorkDirectory">工作目录</param>
    /// <param name="exeName">可执行文件名</param>
    /// <param name="parameters">参数</param>
    /// <param name="result">输出结果</param>
    /// <param name="timeoutSeconds">超时时间（秒），默认 3 秒</param>
    /// <returns>执行是否成功</returns>
    public bool RunExeAndReadResultWithTimeLimit(string exeWorkDirectory, string exeName, string parameters, out string result, int timeoutSeconds = 3)
    {
        result = string.Empty;
        try
        {
            using (var process = new Process())
            {
                WriteLog($"ExecuteDir: {exeWorkDirectory}{exeName}, Parameters: {parameters}, Timeout: {timeoutSeconds}s");
                ConfigureProcessStartInfo(process.StartInfo, exeWorkDirectory, exeName, parameters);

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        WriteLog($"ErrorDataReceived: {e.Data}");
                };

                process.Start();

                try
                {
                    var outputResult = new StringBuilder();
                    bool finished = false;
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            while (!process.HasExited)
                            {
                                var line = process.StandardOutput.ReadLine();
                                if (line != null)
                                {
                                    outputResult.Append(line);
                                    WriteLog($"ProcessOutput: {line}");
                                }
                            }

                            var remaining = process.StandardOutput.ReadToEnd();
                            outputResult.Append(remaining);
                            if (!string.IsNullOrEmpty(remaining))
                                WriteLog($"ReadEnd: {remaining}");

                            process.StandardOutput.Close();
                        }
                        catch (Exception ex)
                        {
                            WriteLog($"Error reading output: {ex.Message}");
                        }
                    });

                    finished = task.Wait(TimeSpan.FromSeconds(timeoutSeconds));

                    if (!finished && !process.HasExited)
                    {
                        WriteLog("Process timeout, killing...");
                        process.Kill();
                        return false;
                    }

                    result = outputResult.ToString();
                    process.WaitForExit();
                    WriteLog($"ProcessExitCode: {process.ExitCode}");
                    
                    if (process.ExitCode != 0)
                    {
                        WriteLog($"Process failed with exit code: {process.ExitCode}");
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    WriteLog($"Error executing process: {ex.Message}");
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            WriteLog($"Error in RunExeAndReadResultWithTimeLimit: {ex.Message}");
            return false;
        }
    }

    /// <summary>杀死指定名称的所有进程</summary>
    public static void KillProcessByName(string processName)
    {
        try
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                using (process)
                {
                    try
                    {
                        if (process.CloseMainWindow())
                        {
                            if (!process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds))
                            {
                                process.Kill();
                            }
                        }
                        else
                        {
                            process.Kill();
                        }

                        Console.WriteLine($"Successfully killed process: {processName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error killing process {processName}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in KillProcessByName: {ex.Message}");
        }
    }

    /// <summary>杀死指定 ID 的进程</summary>
    public static void KillProcessByID(int processID)
    {
        try
        {
            var process = Process.GetProcessById(processID);
            using (process)
            {
                try
                {
                    if (process.CloseMainWindow())
                    {
                        if (!process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds))
                        {
                            process.Kill();
                        }
                    }
                    else
                    {
                        process.Kill();
                    }

                    Console.WriteLine($"Successfully killed process ID: {processID}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error killing process ID {processID}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in KillProcessByID: {ex.Message}");
        }
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>释放资源的受保护方法</summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_process != null)
                {
                    if (!_process.HasExited)
                    {
                        try
                        {
                            _process.Kill();
                        }
                        catch { }
                    }
                    _process.Dispose();
                    _process = null;
                }
            }
            _disposed = true;
        }
    }

    /// <summary>析构函数</summary>
    ~ProcessHelper()
    {
        Dispose(false);
    }
}