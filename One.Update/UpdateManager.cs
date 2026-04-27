using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using One.Update.Abstractions.DTOs;
using One.Update.Abstractions.Enums;
using One.Update.Abstractions.Interfaces;

namespace One.Update;

/// <summary>
/// 更新管理器实现。
/// 编排整个更新流程的核心组件，协调检查、下载、备份、安装等步骤。
/// 
/// 完整更新流程（两阶段）：
///   1. CheckForUpdateAsync → 向服务端查询可用更新（主程序 + 升级程序）
///   2. 用户确认（非强制更新时）→ shouldUpdate 回调
///   3. PrepareUpgradeSelfAsync → 下载并替换升级程序文件（主程序负责）
///   4. PrepareUpdateAsync → 下载更新包 + 解压 + 备份 + 构建 InstallerArgs
///   5. LaunchInstaller → 启动升级程序 + 退出当前进程
/// 
/// 升级程序启动后执行：
///   覆盖文件 → 清理临时文件 → 启动主程序
/// </summary>
public class UpdateManager : IUpdateManager
{
    private readonly IUpdateChecker _checker;
    private readonly IUpdateDownloader _downloader;
    private readonly IBlackListFilter _blackListFilter;
    private readonly HttpClient _reportHttpClient;

    /// <summary>
    /// 更新前自定义检查方法列表。
    /// 在启动升级程序前执行，用于检查当前软件环境是否满足更新条件。
    /// 如果任何方法返回 false 或抛出异常，更新将被中止。
    /// 
    /// 典型用途：
    ///   - 检查磁盘空间是否充足
    ///   - 检查必要的运行时依赖是否安装
    ///   - 检查是否有其他进程占用关键文件
    /// </summary>
    public List<Func<bool>> PreUpdateChecks { get; } = new();

    /// <summary>
    /// 初始化更新管理器。
    /// </summary>
    /// <param name="checker">更新检查器实例</param>
    /// <param name="downloader">更新包下载器实例</param>
    /// <param name="blackListFilter">黑名单过滤器实例</param>
    /// <param name="reportHttpClient">用于上报更新结果的 HTTP 客户端</param>
    public UpdateManager(
        IUpdateChecker checker,
        IUpdateDownloader downloader,
        IBlackListFilter blackListFilter,
        HttpClient reportHttpClient)
    {
        _checker = checker;
        _downloader = downloader;
        _blackListFilter = blackListFilter;
        _reportHttpClient = reportHttpClient;
    }

    /// <inheritdoc />
    public async Task<UpdateCheckResponse> CheckForUpdateAsync(
        string serverUrl,
        string currentVersion,
        int appType,
        string appKey)
    {
        var request = new UpdateCheckRequest
        {
            Version = currentVersion,
            AppType = appType,
            AppKey = appKey,
            Platform = (int)PlatformType.Windows
        };

        return await _checker.CheckAsync(serverUrl, request);
    }

    /// <inheritdoc />
    public async Task<InstallerArgs> PrepareUpdateAsync(
        List<UpdatePackageInfo> packages,
        string installPath,
        string currentVersion,
        string? installerExeName = null,
        IProgress<double>? progress = null,
        Action<InstallStage>? stageChanged = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(installPath);

        if (!ExecutePreUpdateChecks())
            throw new InvalidOperationException("Pre-update checks failed. Update aborted.");

        var tempPath = Path.Combine(Path.GetTempPath(), $"oneupdate_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(tempPath);

        var backupPath = Path.Combine(Path.GetTempPath(), $"oneupdate_backup_{currentVersion}");

        stageChanged?.Invoke(InstallStage.Backup);
        BackupService.Backup(installPath, backupPath, _blackListFilter.SkipDirectoryNames, _blackListFilter);

        stageChanged?.Invoke(InstallStage.Download);
        var downloadedFiles = await _downloader.DownloadAllAsync(packages, tempPath, progress, cancellationToken);

        stageChanged?.Invoke(InstallStage.Extract);
        var extractPath = Path.Combine(tempPath, "extracted");
        Directory.CreateDirectory(extractPath);

        foreach (var packageFile in downloadedFiles)
        {
            ZipFile.ExtractToDirectory(packageFile, extractPath, overwriteFiles: true);
        }

        if (!string.IsNullOrWhiteSpace(installerExeName))
        {
            ApplyInstallerUpdate(extractPath, installPath, installerExeName);
        }

        var packageFiles = downloadedFiles
            .Select(f => Path.GetFileName(f) ?? f)
            .ToList();

        var targetVersion = packages
            .OrderByDescending(p => p.ReleaseDate)
            .FirstOrDefault()?.Version ?? "unknown";

        var args = new InstallerArgs
        {
            InstallPath = installPath,
            CurrentVersion = currentVersion,
            TargetVersion = targetVersion,
            Format = packages.FirstOrDefault()?.Format?.TrimStart('.') ?? "zip",
            Packages = packages,
            PackageFiles = packageFiles,
            TempPath = tempPath,
            BackupPath = backupPath,
            RecordIds = packages.Select(p => p.RecordId).ToList(),
            BlackFiles = _blackListFilter.BlackFileNames.ToList(),
            BlackFormats = _blackListFilter.BlackFormats.ToList(),
            SkipDirectories = _blackListFilter.SkipDirectoryNames.ToList()
        };

        WriteUpdatePendingMarker(new UpdatePendingMarker
        {
            BackupPath = backupPath,
            TargetVersion = targetVersion,
            InstallPath = installPath
        });

        return args;
    }

    /// <summary>
    /// 下载并替换升级程序自身的文件。
    /// 由主程序调用：主程序运行中可以安全替换升级程序的文件。
    /// 下载后直接解压覆盖到安装目录中升级程序所在位置。
    /// </summary>
    /// <param name="packages">升级程序的更新包列表</param>
    /// <param name="installPath">安装目录路径</param>
    /// <param name="progress">下载进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task PrepareUpgradeSelfAsync(
        List<UpdatePackageInfo> packages,
        string installPath,
        IProgress<double>? progress = null,
        Action<InstallStage>? stageChanged = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(installPath);

        var tempPath = Path.Combine(Path.GetTempPath(), $"oneupdate_upgrade_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(tempPath);

        try
        {
            stageChanged?.Invoke(InstallStage.Download);
            var downloadedFiles = await _downloader.DownloadAllAsync(packages, tempPath, progress, cancellationToken);

            stageChanged?.Invoke(InstallStage.Extract);
            var extractPath = Path.Combine(tempPath, "extracted");
            Directory.CreateDirectory(extractPath);

            foreach (var packageFile in downloadedFiles)
            {
                ZipFile.ExtractToDirectory(packageFile, extractPath, overwriteFiles: true);
            }

            foreach (var file in Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(extractPath, file);
                var targetPath = Path.Combine(installPath, relativePath);

                var targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                if (File.Exists(targetPath))
                {
                    File.SetAttributes(targetPath, FileAttributes.Normal);
                    File.Delete(targetPath);
                }

                File.Move(file, targetPath);
            }
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempPath))
                    BackupService.DeleteDirectory(tempPath);
            }
            catch
            {
                // 清理失败不影响主流程
            }
        }
    }

    /// <inheritdoc />
    public void LaunchInstaller(InstallerArgs args, string installerExeName)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(installerExeName);

        var argsJson = JsonSerializer.Serialize(args);
        var argsBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(argsJson));

        var installerPath = Path.Combine(args.InstallPath, installerExeName);
        if (!File.Exists(installerPath))
            throw new FileNotFoundException($"Installer not found: {installerPath}");

        Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = $"--update-args \"{argsBase64}\"",
            UseShellExecute = true
        });

        Environment.Exit(0);
    }

    /// <inheritdoc />
    public async Task PerformUpdateAsync(
        string serverUrl,
        string currentVersion,
        int appType,
        string appKey,
        string installPath,
        string installerExeName,
        Func<List<UpdatePackageInfo>, Task<bool>>? shouldUpdate = null,
        Func<Task>? onUpdateUnavailable = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var checkResponse = await CheckForUpdateAsync(serverUrl, currentVersion, appType, appKey);

        var hasUpdate = checkResponse.Code == 200 && checkResponse.Body is { Count: > 0 };

        if (!hasUpdate)
        {
            if (onUpdateUnavailable != null)
                await onUpdateUnavailable();
            return;
        }

        var packages = checkResponse.Body!;
        var isForcibly = packages.Any(p => p.IsForcibly == true);

        if (!isForcibly && shouldUpdate != null)
        {
            var userConfirmed = await shouldUpdate(packages);
            if (!userConfirmed)
                return;
        }

        var args = await PrepareUpdateAsync(packages, installPath, currentVersion, installerExeName, progress, null, cancellationToken);

        args.AppName = Path.GetFileNameWithoutExtension(installerExeName)
            .Replace(".Upgrade", ".Avalonia", StringComparison.OrdinalIgnoreCase)
            + ".dll";
        args.AppKey = appKey;
        args.ReportUrl = DeriveReportUrl(serverUrl);

        LaunchInstaller(args, installerExeName);
    }

    /// <inheritdoc />
    public async Task ReportAsync(string reportUrl, UpdateReportRequest request)
    {
        ArgumentNullException.ThrowIfNull(reportUrl);
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _reportHttpClient.PostAsJsonAsync(reportUrl, request);
        }
        catch
        {
            // 上报失败不影响主流程
        }
    }

    /// <summary>
    /// 执行更新前自定义检查方法集合。
    /// 所有方法必须返回 true 才允许继续更新。
    /// </summary>
    /// <returns>所有检查是否通过</returns>
    private bool ExecutePreUpdateChecks()
    {
        foreach (var check in PreUpdateChecks)
        {
            try
            {
                if (!check())
                    return false;
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    private static void ApplyInstallerUpdate(string extractPath, string installPath, string installerExeName)
    {
        var installerBaseName = Path.GetFileNameWithoutExtension(installerExeName);

        var filesToCopy = new List<string>
        {
            installerExeName,
            $"{installerBaseName}.dll",
            $"{installerBaseName}.deps.json",
            $"{installerBaseName}.runtimeconfig.json",
            "One.Update.dll",
            "One.Update.Abstractions.dll"
        };

        foreach (var fileName in filesToCopy)
        {
            var sourcePath = Path.Combine(extractPath, fileName);
            if (!File.Exists(sourcePath))
                continue;

            var targetPath = Path.Combine(installPath, fileName);

            if (File.Exists(targetPath))
            {
                File.SetAttributes(targetPath, FileAttributes.Normal);
                try
                {
                    File.Delete(targetPath);
                }
                catch
                {
                    var oldFilePath = targetPath + ".old";
                    if (File.Exists(oldFilePath))
                    {
                        File.SetAttributes(oldFilePath, FileAttributes.Normal);
                        File.Delete(oldFilePath);
                    }
                    File.Move(targetPath, oldFilePath);
                }
            }

            File.Copy(sourcePath, targetPath, true);
        }
    }

    /// <summary>
    /// 写入更新待决标记文件。
    /// 标记文件用于崩溃回滚检测：如果更新后主程序无法正常启动，
    /// 升级程序将检测到此标记并自动回滚。
    /// </summary>
    /// <param name="marker">待决标记信息</param>
    private static void WriteUpdatePendingMarker(UpdatePendingMarker marker)
    {
        var markerPath = Path.Combine(Path.GetTempPath(), "oneupdate_pending.json");
        var json = JsonSerializer.Serialize(marker);
        File.WriteAllText(markerPath, json);
    }

    /// <summary>
    /// 检查是否存在更新待决标记。
    /// 主程序启动时应调用此方法，如果标记存在且已过期（超过 30 分钟），
    /// 说明上次更新后主程序无法正常启动，需要回滚。
    /// </summary>
    /// <returns>标记信息，不存在则返回 null</returns>
    public static UpdatePendingMarker? CheckUpdatePendingMarker()
    {
        var markerPath = Path.Combine(Path.GetTempPath(), "oneupdate_pending.json");
        if (!File.Exists(markerPath))
            return null;

        try
        {
            var json = File.ReadAllText(markerPath);
            return JsonSerializer.Deserialize<UpdatePendingMarker>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 根据 ServerUrl 推导 ReportUrl。
    /// 将 URL 路径的最后一段替换为 "report"。
    /// 如 /api/upgrade/check → /api/upgrade/report
    /// </summary>
    public static string DeriveReportUrl(string serverUrl)
    {
        var lastSlashIndex = serverUrl.LastIndexOf('/');
        if (lastSlashIndex < 0)
            return serverUrl;

        return serverUrl[..(lastSlashIndex + 1)] + "report";
    }

    /// <summary>
    /// 清除更新待决标记。
    /// 主程序启动成功后调用，表示更新已完成，无需回滚。
    /// 同时删除备份目录以释放磁盘空间。
    /// 
    /// 注意：必须先读取标记内容（获取备份路径），再删除标记文件，再清理备份。
    /// </summary>
    public static void ClearUpdatePendingMarker()
    {
        var markerPath = Path.Combine(Path.GetTempPath(), "oneupdate_pending.json");

        var marker = CheckUpdatePendingMarker();

        if (File.Exists(markerPath))
        {
            File.SetAttributes(markerPath, FileAttributes.Normal);
            File.Delete(markerPath);
        }

        if (marker != null && Directory.Exists(marker.BackupPath))
        {
            try
            {
                BackupService.DeleteDirectory(marker.BackupPath);
            }
            catch
            {
                // 清理备份失败不影响主流程
            }
        }
    }
}
