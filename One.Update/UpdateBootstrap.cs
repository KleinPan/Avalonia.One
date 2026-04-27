using System.Diagnostics;
using One.Update.Abstractions.DTOs;
using One.Update.Abstractions.Enums;
using One.Update.Abstractions.Interfaces;

namespace One.Update;

public class UpdateBootstrap
{
    private UpdateConfig? _config;
    private Func<UpdateInfoEventArgs, Task<bool>>? _updateConfirmCallback;
    private Action<UpdateInfoEventArgs>? _noUpdateListener;
    private Action<DownloadProgressEventArgs>? _downloadProgressListener;
    private Action<InstallStage>? _installStageListener;
    private Action<Exception>? _exceptionListener;
    private readonly List<Func<bool>> _customChecks = new();
    private HttpClient? _externalHttpClient;
    private CancellationToken _externalCancellationToken;

    private long _lastProgressBytes;
    private long _speedSampleBytes;
    private readonly Stopwatch _speedStopwatch = new();

    public UpdateBootstrap SetCancellationToken(CancellationToken cancellationToken)
    {
        _externalCancellationToken = cancellationToken;
        return this;
    }

    public UpdateBootstrap SetConfig(UpdateConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
        return this;
    }

    public UpdateBootstrap SetHttpClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _externalHttpClient = httpClient;
        return this;
    }

    public UpdateBootstrap AddListenerUpdateConfirm(Func<UpdateInfoEventArgs, Task<bool>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _updateConfirmCallback = func;
        return this;
    }

    public UpdateBootstrap AddListenerNoUpdate(Action<UpdateInfoEventArgs> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _noUpdateListener = action;
        return this;
    }

    public UpdateBootstrap AddListenerDownloadProgress(Action<DownloadProgressEventArgs> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _downloadProgressListener = action;
        return this;
    }

    public UpdateBootstrap AddListenerInstallStage(Action<InstallStage> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _installStageListener = action;
        return this;
    }

    public UpdateBootstrap AddListenerException(Action<Exception> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _exceptionListener = action;
        return this;
    }

    public UpdateBootstrap AddCustomCheck(Func<bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);
        _customChecks.Add(check);
        return this;
    }

    public async Task<UpdateBootstrap> LaunchAsync(CancellationToken cancellationToken = default)
    {
        if (_config == null)
            throw new InvalidOperationException("UpdateConfig is not set. Call SetConfig() first.");

        _config.Validate();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _externalCancellationToken);
        var ct = linkedCts.Token;

        var ownHttpClient = _externalHttpClient == null;
        var httpClient = _externalHttpClient ?? new HttpClient();

        try
        {
            var checker = new UpdateChecker(httpClient);
            var downloader = new UpdateDownloader(httpClient, _config.MaxDownloadSpeed);
            var blackListFilter = new BlackListFilter();

            if (_config.BlackFiles.Count > 0)
                blackListFilter.AddBlackFiles(_config.BlackFiles);
            if (_config.BlackFormats.Count > 0)
                blackListFilter.AddBlackFormats(_config.BlackFormats);
            if (_config.SkipDirectories.Count > 0)
                blackListFilter.AddSkipDirectories(_config.SkipDirectories);

            var updateManager = new UpdateManager(checker, downloader, blackListFilter, httpClient);
            updateManager.PreUpdateChecks.AddRange(_customChecks);

            var checkResponse = await updateManager.CheckForUpdateAsync(
                _config.ServerUrl, _config.CurrentVersion, _config.AppType, _config.AppKey);

            var updateInfoArgs = new UpdateInfoEventArgs(checkResponse);

            if (!updateInfoArgs.HasUpdate)
            {
                _noUpdateListener?.Invoke(updateInfoArgs);
                return this;
            }

            var confirmed = await ConfirmUpdateAsync(updateInfoArgs.IsForcibly, updateInfoArgs);
            if (!confirmed)
                return this;

            var reportUrl = _config.ReportUrl ?? UpdateManager.DeriveReportUrl(_config.ServerUrl);

            _speedStopwatch.Restart();
            _speedSampleBytes = 0;
            _lastProgressBytes = 0;

            var args = await updateManager.PrepareUpdateAsync(
                checkResponse.Body!, _config.InstallPath, _config.CurrentVersion,
                _config.InstallerExeName,
                new Progress<double>(p => ReportDownloadProgress(p, updateInfoArgs.LatestVersion)),
                stage => _installStageListener?.Invoke(stage),
                ct);

            args.AppName = _config.AppName;
            args.AppKey = _config.AppKey;
            args.ReportUrl = reportUrl;
            args.InstallerExeName = _config.InstallerExeName;

            updateManager.LaunchInstaller(args, _config.InstallerExeName);
        }
        catch (Exception ex)
        {
            _exceptionListener?.Invoke(ex);
        }
        finally
        {
            if (ownHttpClient)
                httpClient.Dispose();
        }

        return this;
    }

    private async Task<bool> ConfirmUpdateAsync(bool isForcibly, UpdateInfoEventArgs updateInfo)
    {
        if (isForcibly)
            return true;

        if (_updateConfirmCallback == null)
            return true;

        return await _updateConfirmCallback(updateInfo);
    }

    private void ReportDownloadProgress(double progress, string? version)
    {
        long bytesPerSecond = 0;
        TimeSpan? remainingTime = null;

        var currentBytes = (long)(progress * 1_000_000);
        _speedSampleBytes += currentBytes - _lastProgressBytes;
        _lastProgressBytes = currentBytes;

        if (_speedStopwatch.Elapsed >= TimeSpan.FromSeconds(1) && _speedSampleBytes > 0)
        {
            bytesPerSecond = (long)(_speedSampleBytes / _speedStopwatch.Elapsed.TotalSeconds);

            if (bytesPerSecond > 0 && progress is > 0 and < 1.0)
            {
                var remainingBytes = (1.0 - progress) / progress * _speedSampleBytes * _speedStopwatch.Elapsed.TotalSeconds;
                remainingTime = TimeSpan.FromSeconds(remainingBytes / bytesPerSecond);
            }

            _speedSampleBytes = 0;
            _speedStopwatch.Restart();
        }

        _downloadProgressListener?.Invoke(new DownloadProgressEventArgs(progress, version, bytesPerSecond, remainingTime));
    }
}
