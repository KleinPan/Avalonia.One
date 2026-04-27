using System.Diagnostics;
using System.Security.Cryptography;
using One.Update.Abstractions.DTOs;
using One.Update.Abstractions.Interfaces;

namespace One.Update;

/// <summary>
/// 更新包下载器实现。
/// 支持断点续传、进度报告和 Hash 校验。
/// 
/// 断点续传实现：
///   1. 下载时写入 {filename}.temp 临时文件
///   2. 检查临时文件大小作为续传起始位置
///   3. 发送 HTTP Range 请求获取剩余内容
///   4. 下载完成后校验 Hash，失败则删除临时文件
///   5. Hash 校验通过后重命名为正式文件名
/// </summary>
public class UpdateDownloader : IUpdateDownloader
{
    private readonly HttpClient _httpClient;
    private readonly long? _maxBytesPerSecond;

    /// <summary>
    /// 初始化更新包下载器。
    /// </summary>
    /// <param name="httpClient">HTTP 客户端实例</param>
    /// <param name="maxBytesPerSecond">最大下载速度（字节/秒），null 或 0 表示不限速</param>
    public UpdateDownloader(HttpClient httpClient, long? maxBytesPerSecond = null)
    {
        _httpClient = httpClient;
        _maxBytesPerSecond = maxBytesPerSecond > 0 ? maxBytesPerSecond : null;
    }

    /// <inheritdoc />
    public async Task<string> DownloadAsync(
        UpdatePackageInfo packageInfo,
        string targetDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packageInfo);
        ArgumentNullException.ThrowIfNull(targetDirectory);

        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        var format = packageInfo.Format.TrimStart('.');
        var fileName = $"{packageInfo.Name}.{format}";
        var targetPath = Path.Combine(targetDirectory, fileName);
        var tempPath = targetPath + ".temp";

        var startPosition = GetExistingFileSize(tempPath);

        using var request = new HttpRequestMessage(HttpMethod.Get, packageInfo.Url);
        if (startPosition > 0)
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startPosition, null);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength ?? 0;
        var totalBytes = startPosition > 0
            ? startPosition + contentLength
            : (packageInfo.Size > 0 ? packageInfo.Size.Value : contentLength);

        var buffer = new byte[81920];
        var totalRead = startPosition;
        var speedLimiter = _maxBytesPerSecond.HasValue ? new ThrottleStrategy(_maxBytesPerSecond.Value) : null;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        {
            await using var fileStream = new FileStream(
                tempPath,
                startPosition > 0 ? FileMode.Append : FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                buffer.Length,
                FileOptions.Asynchronous);

            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (totalBytes > 0)
                    progress?.Report((double)totalRead / totalBytes);

                if (speedLimiter != null)
                    await speedLimiter.ThrottleAsync(bytesRead, cancellationToken);
            }
            await fileStream.FlushAsync(cancellationToken);
        }

        if (!string.IsNullOrEmpty(packageInfo.Hash))
        {
            var actualHash = ComputeFileHash(tempPath);
            if (!string.Equals(actualHash, packageInfo.Hash, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(tempPath);
                throw new InvalidOperationException(
                    $"Hash verification failed. Expected: {packageInfo.Hash}, Actual: {actualHash}");
            }
        }

        if (File.Exists(targetPath))
        {
            File.SetAttributes(targetPath, FileAttributes.Normal);
            File.Delete(targetPath);
        }

        File.Move(tempPath, targetPath);

        return targetPath;
    }

    /// <inheritdoc />
    /// <remarks>
    /// 进度报告按字节计算总体进度：所有包的总已下载字节 / 所有包的总字节数。
    /// 总字节数优先使用 packageInfo.Size，如果为空则从 HTTP 响应头 Content-Length 获取。
    /// </remarks>
    private const int MaxRetryCount = 3;

    public async Task<List<string>> DownloadAllAsync(
        List<UpdatePackageInfo> packages,
        string targetDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packages);

        var downloadedFiles = new List<string>();
        var packageIndex = 0;

        foreach (var package in packages)
        {
            var currentPackageIndex = packageIndex;
            var perPackageProgress = progress != null
                ? new Progress<double>(ratio =>
                {
                    if (packages.Count == 1)
                    {
                        progress.Report(ratio);
                    }
                    else
                    {
                        var completedRatio = (double)currentPackageIndex / packages.Count;
                        var currentRatio = ratio / packages.Count;
                        progress.Report(completedRatio + currentRatio);
                    }
                })
                : null;

            var filePath = await DownloadWithRetryAsync(package, targetDirectory, perPackageProgress, cancellationToken);
            downloadedFiles.Add(filePath);

            packageIndex++;
        }

        return downloadedFiles;
    }

    private async Task<string> DownloadWithRetryAsync(
        UpdatePackageInfo packageInfo,
        string targetDirectory,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxRetryCount; attempt++)
        {
            try
            {
                return await DownloadAsync(packageInfo, targetDirectory, progress, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidOperationException) when (attempt < MaxRetryCount)
            {
                await Task.Delay(1000 * attempt, cancellationToken);
            }
            catch (HttpRequestException) when (attempt < MaxRetryCount)
            {
                await Task.Delay(1000 * attempt, cancellationToken);
            }
        }

        return await DownloadAsync(packageInfo, targetDirectory, progress, cancellationToken);
    }

    /// <summary>
    /// 获取已存在的临时文件大小，用于断点续传。
    /// </summary>
    /// <param name="tempPath">临时文件路径</param>
    /// <returns>文件大小（字节），文件不存在则返回 0</returns>
    private static long GetExistingFileSize(string tempPath)
    {
        if (!File.Exists(tempPath))
            return 0;

        var fileInfo = new FileInfo(tempPath);
        return fileInfo.Length;
    }

    /// <summary>
    /// 计算文件的 SHA256 Hash 值。
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>小写的十六进制 Hash 字符串</returns>
    private static string ComputeFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// 下载限速策略。
    /// 通过令牌桶算法控制下载速度：记录每个时间窗口内已传输的字节数，
    /// 如果超过限制则等待至下一个时间窗口。
    /// </summary>
    private class ThrottleStrategy
    {
        private readonly long _maxBytesPerTick;
        private readonly long _ticksPerSecond;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private long _bytesInWindow;

        public ThrottleStrategy(long maxBytesPerSecond)
        {
            _ticksPerSecond = Stopwatch.Frequency / 10;
            _maxBytesPerTick = maxBytesPerSecond / 10;
        }

        public async Task ThrottleAsync(int bytesTransferred, CancellationToken cancellationToken)
        {
            _bytesInWindow += bytesTransferred;

            if (_bytesInWindow >= _maxBytesPerTick)
            {
                var elapsed = _stopwatch.ElapsedTicks;
                if (elapsed < _ticksPerSecond)
                {
                    var delayTicks = _ticksPerSecond - elapsed;
                    var delayMs = (int)(delayTicks * 1000.0 / Stopwatch.Frequency);
                    if (delayMs > 0)
                        await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                }

                _stopwatch.Restart();
                _bytesInWindow = 0;
            }
        }
    }
}
