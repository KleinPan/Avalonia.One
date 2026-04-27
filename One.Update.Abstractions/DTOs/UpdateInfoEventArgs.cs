namespace One.Update.Abstractions.DTOs;

/// <summary>
/// 更新信息事件参数。
/// 在版本检查完成后触发，携带完整的更新检查响应信息。
/// 调用方可通过此参数获取新版本号、发布日期、更新日志等详情，用于弹窗展示。
///
/// 使用场景：
///   - AddListenerUpdateConfirm 回调中获取更新详情并弹窗确认
///   - AddListenerNoUpdate 回调中获取检查结果
/// </summary>
public class UpdateInfoEventArgs : EventArgs
{
    /// <summary>
    /// 更新检查响应。
    /// Code = 200 且 Body 非空表示有可用更新。
    /// </summary>
    public UpdateCheckResponse Response { get; }

    /// <summary>
    /// 是否存在可用更新。
    /// 等价于 Response.Code == 200 &amp;&amp; Response.Body?.Count > 0
    /// </summary>
    public bool HasUpdate { get; }

    /// <summary>
    /// 是否为强制更新。
    /// 如果更新包列表中任何一个包标记了 IsForcibly = true，则为强制更新。
    /// </summary>
    public bool IsForcibly { get; }

    /// <summary>
    /// 最新版本号。
    /// 从更新包列表中按 ReleaseDate 降序取第一个包的 Version。
    /// 无更新时为 null。
    /// </summary>
    public string? LatestVersion { get; }

    public UpdateInfoEventArgs(UpdateCheckResponse response)
    {
        Response = response ?? throw new ArgumentNullException(nameof(response));
        HasUpdate = response.Code == 200 && response.Body is { Count: > 0 };
        IsForcibly = response.Body?.Any(p => p.IsForcibly == true) == true;
        LatestVersion = response.Body?
            .OrderByDescending(p => p.ReleaseDate)
            .FirstOrDefault()?.Version;
    }
}
