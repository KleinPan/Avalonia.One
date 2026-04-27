using One.Update.Abstractions.DTOs;
using One.Update.Abstractions.Enums;

namespace One.Update.Abstractions.Interfaces;

/// <summary>
/// 更新管理器接口。
/// 编排整个更新流程的核心组件，协调检查、下载、备份、安装等步骤。
/// 
/// 典型调用流程：
///   1. 调用 CheckForUpdateAsync 检查更新
///   2. 根据返回结果决定是否更新（强制更新跳过确认）
///   3. 调用 PrepareUpdateAsync 备份 + 下载更新包
///   4. 调用 LaunchInstallerAsync 启动升级程序
///   5. 主程序退出
/// 
/// 也可以使用便捷方法 PerformUpdateAsync，一次性完成上述所有步骤。
/// </summary>
public interface IUpdateManager
{
    /// <summary>
    /// 检查是否有可用更新。
    /// </summary>
    /// <param name="serverUrl">服务端更新检查接口地址</param>
    /// <param name="currentVersion">当前版本号</param>
    /// <param name="appType">应用类型（主程序/升级程序）</param>
    /// <param name="appKey">应用密钥</param>
    /// <returns>更新检查响应</returns>
    Task<UpdateCheckResponse> CheckForUpdateAsync(
        string serverUrl,
        string currentVersion,
        int appType,
        string appKey);

    /// <summary>
    /// 准备更新：备份当前文件 + 下载更新包。
    /// 此方法不启动升级程序，调用方可在此之后执行自定义操作。
    /// </summary>
    /// <param name="packages">要下载的更新包列表</param>
    /// <param name="installPath">安装目录路径</param>
    /// <param name="currentVersion">当前版本号（用于备份目录命名）</param>
    /// <param name="progress">下载进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>安装参数对象，包含所有下载和备份信息</returns>
    Task<InstallerArgs> PrepareUpdateAsync(
        List<UpdatePackageInfo> packages,
        string installPath,
        string currentVersion,
        string? installerExeName = null,
        IProgress<double>? progress = null,
        Action<InstallStage>? stageChanged = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 启动升级程序并退出当前进程。
    /// </summary>
    /// <param name="args">安装参数，由 PrepareUpdateAsync 返回</param>
    /// <param name="installerExeName">升级程序可执行文件名，如 "Carina.Upgrade.exe"</param>
    void LaunchInstaller(InstallerArgs args, string installerExeName);

    /// <summary>
    /// 便捷方法：一次性完成检查 → 确认 → 下载 → 安装的全流程。
    /// 
    /// 流程控制：
    ///   - 无更新：调用 onUpdateUnavailable 回调
    ///   - 有更新且非强制：调用 shouldUpdate 回调，由用户决定是否更新
    ///   - 有更新且强制：跳过用户确认，直接更新
    ///   - 用户确认更新或强制更新：执行备份 → 下载 → 启动升级程序 → 退出
    /// </summary>
    /// <param name="serverUrl">服务端更新检查接口地址</param>
    /// <param name="currentVersion">当前版本号</param>
    /// <param name="appType">应用类型</param>
    /// <param name="appKey">应用密钥</param>
    /// <param name="installPath">安装目录路径</param>
    /// <param name="installerExeName">升级程序可执行文件名</param>
    /// <param name="shouldUpdate">
    /// 用户确认回调，返回 true 表示同意更新，false 表示跳过本次更新。
    /// 仅在非强制更新时调用；强制更新时此回调被忽略。
    /// 参数为更新信息列表，供弹窗展示。
    /// </param>
    /// <param name="onUpdateUnavailable">无可用更新时的回调</param>
    /// <param name="progress">下载进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PerformUpdateAsync(
        string serverUrl,
        string currentVersion,
        int appType,
        string appKey,
        string installPath,
        string installerExeName,
        Func<List<UpdatePackageInfo>, Task<bool>>? shouldUpdate = null,
        Func<Task>? onUpdateUnavailable = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 上报更新结果到服务端。
    /// </summary>
    /// <param name="reportUrl">上报接口地址</param>
    /// <param name="request">上报请求参数</param>
    Task ReportAsync(string reportUrl, UpdateReportRequest request);
}
