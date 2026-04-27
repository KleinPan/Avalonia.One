using System.Net.Http.Json;
using One.Update.Abstractions.DTOs;
using One.Update.Abstractions.Interfaces;

namespace One.Update;

/// <summary>
/// 更新检查器实现。
/// 通过 HTTP POST 请求服务端的更新检查接口，获取可用更新包信息。
/// 
/// 内部使用 IHttpClientFactory 管理的 HttpClient 实例，
/// 支持 SSL 证书自定义验证（用于内网自签名证书场景）。
/// </summary>
public class UpdateChecker : IUpdateChecker
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 初始化更新检查器。
    /// </summary>
    /// <param name="httpClient">
    /// HTTP 客户端实例。建议通过 IHttpClientFactory 创建，
    /// 以避免 Socket 耗尽问题。
    /// </param>
    public UpdateChecker(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    /// <remarks>
    /// 请求超时：使用 HttpClient 的默认超时（100 秒）。
    /// 如果服务端响应的 Code 不为 200，将抛出 HttpRequestException。
    /// </remarks>
    public async Task<UpdateCheckResponse> CheckAsync(string serverUrl, UpdateCheckRequest request)
    {
        ArgumentNullException.ThrowIfNull(serverUrl);
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync(serverUrl, request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UpdateCheckResponse>();

        return result ?? new UpdateCheckResponse { Code = -1, Message = "Failed to deserialize response." };
    }
}
