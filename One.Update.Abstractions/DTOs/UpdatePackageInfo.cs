namespace One.Update.Abstractions.DTOs;

/// <summary>
/// 更新包信息 DTO。
/// 服务端返回的每个可用更新包的详细信息，包含下载地址、版本号、Hash 校验值等。
/// 客户端根据此信息下载更新包并校验完整性。
/// </summary>
public class UpdatePackageInfo
{
    /// <summary>
    /// 更新记录 ID，用于更新结果上报时关联本次更新记录。
    /// 服务端通过此 ID 追踪每个更新包的执行状态。
    /// </summary>
    public int RecordId { get; set; }

    /// <summary>
    /// 更新包文件名（不含扩展名），如 "Carina.Avalonia_3.1.0"。
    /// 下载时将拼接 Format 形成完整文件名。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 更新包的 SHA256 Hash 值，用于下载完成后校验文件完整性。
    /// 如果 Hash 校验失败，客户端将删除已下载的文件并重试。
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// 更新包发布日期，客户端按此字段排序确定最新版本。
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// 更新包下载地址（完整 URL），如 "http://server:5000/packages/Carina.Avalonia_3.1.0.zip"。
    /// 支持断点续传（HTTP Range 请求）。
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 更新包对应的目标版本号，如 "3.1.0"。
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 应用类型，参见 <see cref="Enums.AppType"/>。
    /// </summary>
    public int? AppType { get; set; }

    /// <summary>
    /// 目标平台，参见 <see cref="Enums.PlatformType"/>。
    /// </summary>
    public int? Platform { get; set; }

    /// <summary>
    /// 产品分支标识。
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// 是否强制更新。
    /// 当为 true 时，客户端跳过用户确认步骤，直接执行更新；
    /// 用户无法通过"跳过更新"回调取消本次更新。
    /// </summary>
    public bool? IsForcibly { get; set; }

    /// <summary>
    /// 更新包压缩格式，如 "zip"、"7z"。
    /// 当前仅支持 "zip" 格式。
    /// </summary>
    public string Format { get; set; } = "zip";

    /// <summary>
    /// 更新包文件大小（字节），用于显示下载进度和预估剩余时间。
    /// 如果服务端未提供，客户端将在下载开始后从 HTTP 响应头获取。
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// 更新日志或版本说明，可包含新功能描述、Bug 修复列表等。
    /// 客户端可在更新确认弹窗中展示此信息。
    /// </summary>
    public string? UpdateLog { get; set; }
}
