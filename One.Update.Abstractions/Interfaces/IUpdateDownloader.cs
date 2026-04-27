using One.Update.Abstractions.DTOs;

namespace One.Update.Abstractions.Interfaces;

/// <summary>
/// 更新包下载器接口。
/// 负责从服务端下载更新包文件，支持断点续传和进度报告。
/// 
/// 断点续传原理：
///   1. 下载时先写入 .temp 临时文件
///   2. 如果下载中断，.temp 文件保留已下载的部分
///   3. 下次下载时检查 .temp 文件大小，通过 HTTP Range 请求续传
///   4. 下载完成后 .temp 重命名为正式文件名
/// </summary>
public interface IUpdateDownloader
{
    /// <summary>
    /// 下载更新包文件。
    /// 支持断点续传：如果临时文件已存在，将从上次中断的位置继续下载。
    /// </summary>
    /// <param name="packageInfo">更新包信息，包含下载地址、文件名等</param>
    /// <param name="targetDirectory">下载目标目录，文件将保存到此目录</param>
    /// <param name="progress">
    /// 进度报告回调，参数为下载进度百分比（0.0 ~ 1.0）。
    /// 可为 null，表示不需要进度报告。
    /// </param>
    /// <param name="cancellationToken">取消令牌，用于取消正在进行的下载</param>
    /// <returns>下载完成的文件完整路径</returns>
    /// <exception cref="HttpRequestException">网络请求失败时抛出</exception>
    /// <exception cref="OperationCanceledException">下载被取消时抛出</exception>
    /// <exception cref="InvalidOperationException">Hash 校验失败时抛出</exception>
    Task<string> DownloadAsync(
        UpdatePackageInfo packageInfo,
        string targetDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量下载多个更新包。
    /// 逐个下载，每个包都支持断点续传和进度报告。
    /// </summary>
    /// <param name="packages">更新包信息列表</param>
    /// <param name="targetDirectory">下载目标目录</param>
    /// <param name="progress">
    /// 总体进度报告回调，参数为总体下载进度百分比（0.0 ~ 1.0）。
    /// 计算方式：已完成包数 / 总包数。
    /// </param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>所有下载完成的文件完整路径列表</returns>
    Task<List<string>> DownloadAllAsync(
        List<UpdatePackageInfo> packages,
        string targetDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}
