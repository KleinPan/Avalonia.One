namespace One.Update.Abstractions.DTOs;

/// <summary>
/// 更新检查请求 DTO。
/// 客户端将此对象 POST 到服务端的更新检查接口，服务端根据版本号和应用类型判断是否需要更新。
/// 
/// 协议约定：
///   - 请求路径：POST /api/upgrade/check
///   - Content-Type：application/json
///   - JSON 属性使用 camelCase 命名（通过 System.Text.Json 的 JsonPropertyName 映射）
/// </summary>
public class UpdateCheckRequest
{
    /// <summary>
    /// 客户端当前版本号，格式遵循语义化版本（如 "1.0.0"、"3.2.1"）。
    /// 服务端将此版本与最新版本比较，决定是否返回更新包。
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 应用类型，参见 <see cref="Enums.AppType"/>。
    /// ClientApp(1) = 主程序，UpgradeApp(2) = 升级程序。
    /// </summary>
    public int AppType { get; set; }

    /// <summary>
    /// 应用密钥，用于服务端鉴权。
    /// 每个应用有唯一的 AppSecretKey，防止未授权的更新请求。
    /// </summary>
    public string AppKey { get; set; } = string.Empty;

    /// <summary>
    /// 客户端运行平台，参见 <see cref="Enums.PlatformType"/>。
    /// Windows(1) = Windows 平台，Linux(2) = Linux 平台。
    /// </summary>
    public int Platform { get; set; }

    /// <summary>
    /// 产品分支标识，用于区分同一应用的不同产品线。
    /// 留空表示默认产品分支。
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
}
