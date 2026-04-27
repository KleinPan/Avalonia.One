namespace One.Update.Abstractions.DTOs;

/// <summary>
/// 更新检查响应 DTO。
/// 服务端返回的更新检查结果，包含状态码、消息和可用的更新包列表。
/// 
/// 响应规则：
///   - Code = 200 且 Body 非空：存在可用更新，Body 为更新包列表
///   - Code = 200 且 Body 为空：当前已是最新版本
///   - Code != 200：请求失败，Message 包含错误信息
/// </summary>
public class UpdateCheckResponse
{
    /// <summary>
    /// 响应状态码。
    /// 200 = 成功，其他值 = 失败。
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息，成功时包含描述信息，失败时包含错误原因。
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 可用的更新包列表。
    /// 列表为空或 null 表示当前已是最新版本；
    /// 列表非空时，按 ReleaseDate 排序，最后一个元素为最新版本。
    /// </summary>
    public List<UpdatePackageInfo>? Body { get; set; }
}
