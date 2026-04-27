using One.Update.Abstractions.DTOs;

namespace One.Update.Abstractions.Interfaces;

/// <summary>
/// 更新检查器接口。
/// 负责向服务端查询是否有可用更新，返回更新包信息列表。
/// 
/// 实现类通常通过 HTTP POST 调用服务端的更新检查接口。
/// 调用方（如 UpdateManager）根据返回结果决定后续流程：
///   - 无更新 → 通知用户"已是最新版本"
///   - 有更新 → 弹窗确认 → 下载 → 安装
/// </summary>
public interface IUpdateChecker
{
    /// <summary>
    /// 检查是否有可用更新。
    /// </summary>
    /// <param name="serverUrl">服务端更新检查接口地址，如 "http://server:5000/api/update/check"</param>
    /// <param name="request">更新检查请求参数，包含当前版本号、应用类型、密钥等</param>
    /// <returns>
    /// 更新检查响应。Body 非空表示有可用更新，Body 为空表示当前已是最新版本。
    /// </returns>
    /// <exception cref="HttpRequestException">网络请求失败时抛出</exception>
    /// <exception cref="TaskCanceledException">请求超时时抛出</exception>
    Task<UpdateCheckResponse> CheckAsync(string serverUrl, UpdateCheckRequest request);
}
